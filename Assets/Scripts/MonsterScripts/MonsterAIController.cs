using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

enum MonsterState
{
    Hiding,
    Patrolling,
    Investigating,
    Chasing
}

public class MonsterAIController : MonoBehaviour, IHear, IDamageable
{
    [Header("References")]
    public Transform Player;
    public CharacterController _playerController;

    [Header("Settings")]
    public float AttackDistance;

    [Header("Navigation Settings")]
    [SerializeField] private float NavMaxRadius = 10f;
    [SerializeField] private float NavMinRadius = 0.5f;
    [SerializeField] private float NavAngle = 45f;
    [SerializeField] private float MinEdgeDistance = 0f;
    [SerializeField] private float PatrolTargetTimeout = 4f; // Time until patrol target dismissed

    [Header("Vision Settings")]
    [SerializeField] private float LOSDistance = 100f;
    [SerializeField] private float FOV = 180f;
    public LayerMask ObstacleMask;

    [Header("Patrol Settings")]
    [SerializeField] private float MinStationaryTime = 0.5f;
    [SerializeField] private float MaxStationaryTime = 10f;

    [Header("Investigate Settings")]
    // [SerializeField] private float[] SurveyRadius = {4f, 10f, 15f};
    [SerializeField] private float SearchRadius = 3f;
    [SerializeField] private float MaxSearchRadius = 15f;
    [SerializeField] private float InnerExclusion = 0f; // Percentage of inner radius you want excluded 

    [Header("Chase Settings")]
    [SerializeField] private float ChaseTimeout = 5f;
    [SerializeField] private float AwakenChaseDuration = 10f;

    [Header("Hiding Settings")]
    public Transform[] HidingPoints;
    [SerializeField] private float MinHideDistance = 10f;
    public float WakeUpDuration = 5f;

    [Header("Damage Settings")]
    public float MinStun = 3f;
    public float MaxStun = 8f;
    private float EndStunTime;
    private bool IsStunned => Time.time < EndStunTime;
    

    private NavMeshAgent m_Agent;
    private Animator m_Animator;

    private Transform Target;
    private float TargetPriority;
    private float DistanceFromPlayer;
    private float LastSawPlayer = 0;
    private MonsterState _state = MonsterState.Hiding;
    private bool ReachedTarget = false;
    private float MonsterSpeed = 0f;

    // Patrol variables
    private float LastPatrolTime = 0;
    private float StationTime = 0;

    // Investigate variables
    private Vector3 InvestigateOrigin;
    private bool ReachedOrigin = false;

    // Hiding variables
    private float AwakenTime = 0;

    // Chase variables
    private bool ChaseSearch = false;

    private void Awake()
    {
        m_Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        m_Animator = GetComponent<Animator>();

        MonsterSpeed = m_Agent.speed;

        // Initialise Target obj
        GameObject targetObj = new GameObject("MonsterTarget");
        Target = targetObj.transform;
        Target.position = transform.position;

        Debug.Log($"Monster Target created at: {Target.position}");

        EnterHideState();
    }

    public void TakeDamage()
    {
        if (_state == MonsterState.Hiding)
        {
            // Take damage
            m_Animator.SetTrigger("Damaged");
            m_Animator.SetBool("Hiding", false);

            AwakenTime = Time.time + WakeUpDuration;
            LastSawPlayer = AwakenTime + AwakenChaseDuration;
            _state = MonsterState.Chasing;
            Debug.Log("Took hiding damage");
        }
        else 
        {
            if (Time.time > AwakenTime)
            {
                // Stun Monster
                Debug.Log("I have been shot");
                m_Animator.SetTrigger("Damaged");
                
                // Cap stun duration to max stun
                EndStunTime = Mathf.Min(Time.time + MaxStun, EndStunTime + MinStun);
            }
        }
    }

    // Monster vision cone
    private void LOSCheck(float DistanceFromPlayer)
    {
        Vector3 Origin = transform.position + Vector3.up * 3f;
        Vector3 PlayerDirection = (Player.position + (Vector3.up * 0f) - Origin).normalized;

        // Check if player within LOS range
        if (DistanceFromPlayer > LOSDistance) return;

        // Check if player within vision angle
        float angle = Vector3.Angle(transform.forward, PlayerDirection);
        if (angle > FOV * 0.5) return;

        // Check for vision obstructions with raycast
        if (Physics.Raycast(Origin, PlayerDirection, out RaycastHit hit, LOSDistance, ObstacleMask))
        {
            Debug.DrawLine(Origin, hit.point, Color.red);
            // Debug.Log("Hit: " + hit.transform.name);
            if(hit.transform == Player)
            {
                Debug.Log("Player seen");
                LastSawPlayer = Time.time;
                ReachedTarget = false;
                _state = MonsterState.Chasing;
            }
        }
    }

    private void Update()
    {
        //Debug.Log($"Monster Current State: {_state}");

        DistanceFromPlayer = Vector3.Distance(m_Agent.transform.position, Player.position);

        if (IsStunned)
        {
            m_Agent.speed = 0f;
            m_Agent.isStopped = true;
            m_Animator.SetFloat("Speed", 0f);
            return;
        }

        switch(_state)
        {
            case MonsterState.Patrolling:
                TargetPriority = 0;
                Patrol();
                break;
            case MonsterState.Investigating:
                Investigate();
                break;
            case MonsterState.Chasing:
                Chase();
                break;
            case MonsterState.Hiding:
                Hide();
                break;
        }

        if (_state != MonsterState.Hiding)
        {
            LOSCheck(DistanceFromPlayer);

            if (Vector3.Distance(transform.position, Target.position) < 0.5f)
            {
                // Debug.Log("Target reached");
                ReachedTarget = true;
            }

            // Attack condition
            if (DistanceFromPlayer < AttackDistance)
            {
                m_Agent.isStopped = true;
                m_Animator.SetBool("Attack", true);
            }
            else
            {
                m_Agent.isStopped = false;
                m_Animator.SetBool("Attack", false);
                m_Agent.destination = Target.position;
            }
        }

        m_Animator.SetFloat("Speed", m_Agent.velocity.magnitude);
    }

    public void RespondToSound(Sound sound)
    {
        // Debug.Log("I HEARD THAT!!!!");
        if (_state == MonsterState.Chasing) return;

        if (sound.priority > TargetPriority && _state != MonsterState.Hiding)
        {
            EnterInvestigate(sound.pos);
            Debug.Log("Target to sound set");
        }
        
    }

    private bool IsValidPatrolPoint(Vector3 position, out NavMeshHit hit)
    {
        // Check point is on NavMesh
        if (!NavMesh.SamplePosition(position, out hit, 2f, NavMesh.AllAreas))
            return false;

        // Check distance from nearest edge
        NavMeshHit edgeHit;
        if (NavMesh.FindClosestEdge(hit.position, out edgeHit, NavMesh.AllAreas))
        {
            if (edgeHit.distance < MinEdgeDistance)
                return false;
        }

        return true;
    }

    private float GetPathDistance(Vector3 targetPos)
    {
        NavMeshPath path = new NavMeshPath();

        if (!NavMesh.CalculatePath(transform.position, targetPos, NavMesh.AllAreas, path))
        {
            Debug.LogError("No valid path found");
            return float.MaxValue;
        }

        float pathDistance = 0;
        for (int i = 1; i < path.corners.Length - 1; i++)
            pathDistance += Vector3.Distance(path.corners[i-1], path.corners[i]);
        
        return pathDistance;
    }

    // Finds next target point within a cone, factoring in distance to get to the target
    private bool FindReachablePoint(float maxDistance, float angleRange, out Vector3 result)
    {
        NavMeshHit hit = default;
        int attempts = 0;

        while (attempts < 30)
        {
            float randomAngle = Random.Range(-angleRange * 0.5f, angleRange * 0.5f);
            Vector3 direction = Quaternion.Euler(0, randomAngle, 0) * transform.forward;
            float distance = Random.Range(NavMinRadius, Mathf.Min(NavMaxRadius, maxDistance));
            Vector3 randomPosition = transform.position + direction * distance;

            if (IsValidPatrolPoint(randomPosition, out hit) &&
                GetPathDistance(randomPosition) <= maxDistance)
            {
                result = hit.position;
                return true;
            }

            attempts++;
        }

        result = Vector3.zero;
        return false;
    }

// --- Patrolling ---

    private void Patrol()
    {
        // Debug.Log("Patrol called");
        if (Target == null) 
        { 
            Debug.LogError("Monster Target is NULL"); 
            return; 
        }

        if (Time.time < StationTime)
        {
            //Debug.Log($"Staying stationary for: {StationTime - Time.time}");
            m_Agent.speed = 0f;
            return;
        }

        m_Agent.speed = MonsterSpeed;

        if (ReachedTarget || (PatrolTargetTimeout < Time.time - LastPatrolTime))
        {
            // Sets random position around circle radius of monster as navigation target
            //Vector3 randomPosition = Random.insideUnitSphere * NavMaxRadius;
            //randomPosition += transform.position; // Make random position relative to monster position

            NavMeshHit hit = default;
            int attempts = 0;
            bool validPointFound = false;

            while (attempts < 30)
            {
                // Set random position within cone in front of monster
                float randomAngle = Random.Range(-NavAngle, NavAngle);
                Vector3 direction = Quaternion.Euler(0, randomAngle, 0) * transform.forward;
                float distance = Random.Range(NavMinRadius, NavMaxRadius);
                Vector3 randomPosition = transform.position + direction * distance;

                if (IsValidPatrolPoint(randomPosition, out hit))
                {
                    validPointFound = true;
                    break;
                }

                attempts++;
            }

            if (!validPointFound)
            {
                Debug.LogWarning("Monster AI has made 30 position attempts");
                ReachedTarget = false;
                LastPatrolTime = Time.time;
                return;
            }

            // Set target
            //Debug.Log($"Valid NavMesh point found: {hit.position}");
            Target.position = hit.position;
            ReachedTarget = false;
            LastPatrolTime = Time.time;

            // Set station time
            float StationDuration = Random.Range(MinStationaryTime, MaxStationaryTime);
            StationTime = Time.time + StationDuration;
        }
    }

// --- Investigating ---
    private void EnterInvestigate(Vector3 Origin)
    {
        InvestigateOrigin = Origin;
        Target.position = Origin;
        SearchRadius = 3f;
        ReachedTarget = false;
        _state = MonsterState.Investigating;
    }

    private void Investigate()
    {

        // Debug.Log("Investigate function called");
        if (Target == null)
        { 
            Debug.LogError("Monster Target is NULL"); 
            return; 
        }

        if (InvestigateOrigin == null)
        {
            Debug.LogError("InvestigateOrigin is NULL");
            return;
        }
        
        if (Time.time < StationTime)
        {
            m_Agent.speed = 0f;
            return;
        }

        m_Agent.speed = MonsterSpeed;

        if (ReachedTarget)
        {
            // --- survey growing area around investigation origin ---
            Debug.Log($"Surveying Area Around: {InvestigateOrigin}");

            // Return back to patrol state
            if (SearchRadius > (MaxSearchRadius))
            {
                _state = MonsterState.Patrolling;
                Debug.Log("Back to PATROL");
                return;
            }

            NavMeshHit hit = default;
            Vector3 randomPoint;
            bool validPointFound = false;
            int attempts = 0;

            while (!validPointFound && attempts < 30)
            {
                // Copilot - "How can I pick a point within a search radius, excluding one that is within 0.3x the radius?"
                Vector3 dir = Random.onUnitSphere;
                float range = Random.Range(InnerExclusion * SearchRadius, SearchRadius);
                randomPoint = InvestigateOrigin + dir * range;
                // ---

                if (IsValidPatrolPoint(randomPoint, out hit))
                {
                    validPointFound = true;
                    Debug.Log("Investigation point set");
                }
                attempts++;
            }

            if (!validPointFound)
            {
                Debug.LogWarning("Investigate: No valid point found, expanding search radius");
                SearchRadius += 2f;
                return;
            }

            SearchRadius += 2f;
            Target.position = hit.position;
            ReachedTarget = false;
            LastPatrolTime = Time.time;
            StationTime = Time.time + Random.Range(MinStationaryTime, MaxStationaryTime);
        }

    }

// --- Chase ---
    private void Chase()
    {
        // Pause between hiding state and chase state
        if (Time.time < AwakenTime) return;

        float timeSinceLastSeen = Time.time - LastSawPlayer;

        if (timeSinceLastSeen < 2f)
        {
            Target.position = Player.position;
            return;
        }
        
        // Case 1 (Chase Search): lost sight of player but within chase timeout
        // - Search nearby points player could have reached
        if (timeSinceLastSeen < ChaseTimeout)
        {
            if (ReachedTarget)
            {
                Debug.Log("Case 1 block entered");

                float playerSpeed = _playerController.velocity.magnitude;
                float maxPossibleDistance = timeSinceLastSeen * playerSpeed;
                Vector3 searchTarget;

                if (FindReachablePoint(maxPossibleDistance, FOV, out searchTarget))
                {
                    Target.position = searchTarget;
                    ReachedTarget = false;
                    Debug.Log($"Lost player, searching: {searchTarget}");
                }
            }
            return;
        }

        // Case 2: chase times out
        // - transition to investigate state
        Debug.Log("Chase timed out. Now investigating");
        EnterInvestigate(Target.position);
    }

// --- Hiding ---
    private void EnterHideState()
    {
         
        if (HidingPoints.Length == 0)
        {
            Debug.LogError("No hiding points were input");
            return;
        }

        // Find valid hiding points
        List<Transform> validPoints = new List<Transform>();
        foreach (Transform p in HidingPoints)
        {
            if (Vector3.Distance(p.position, Player.position) >= MinHideDistance)
            {
                validPoints.Add(p);
                Debug.Log($"Hiding point: {p.transform.name}");
            }
        }

        if (validPoints.Count == 0)
        {
            Debug.LogError("No valid hiding points found");
            return;
        }

        // Choose random hiding point
        Transform chosenPoint = validPoints[Random.Range(0, validPoints.Count)];

        m_Agent.Warp(chosenPoint.position);
        transform.rotation = chosenPoint.rotation;
        m_Agent.speed = 0;
        m_Agent.isStopped = true;
        m_Animator.SetBool("Hiding", true);

        _state = MonsterState.Hiding;
    }

    private void Hide()
    {
        m_Agent.speed = 0f;
        m_Agent.isStopped = true;
    }
}
