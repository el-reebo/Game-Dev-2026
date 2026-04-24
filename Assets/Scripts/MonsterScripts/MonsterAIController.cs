using UnityEngine;
using UnityEngine.AI;

enum MonsterState
{
    Hiding,
    Patrolling,
    Investigating,
    Chasing
}

public class MonsterAIController : MonoBehaviour, IHear
{
    [Header("References")]
    public Transform Player;

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
    [SerializeField] private float[] SurveyRadius = {4f, 10f, 15f};

    [Header("Chase Settings")]
    [SerializeField] private float ChaseTimeout = 5f;
    
    private NavMeshAgent m_Agent;
    private Animator m_Animator;

    private Transform Target;
    private float TargetPriority;
    private float DistanceFromPlayer;
    private float LastSawPlayer = 0;
    private MonsterState _state = MonsterState.Patrolling;
    private bool ReachedTarget = false;
    private float MonsterSpeed = 0f;

    // Patrol variables
    private float LastPatrolTime = 0;
    private float StationTime = 0;

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
    }

    private void LOSCheck(float DistanceFromPlayer)
    {
        // Check if player within LOS range
        if (DistanceFromPlayer > LOSDistance) return;

        // Check if player within vision angle

        Vector3 Origin = transform.position + Vector3.up * 3f;
        Vector3 PlayerDirection = (Player.position + (Vector3.up * 0f) - Origin).normalized;
        if (Physics.Raycast(Origin, PlayerDirection, out RaycastHit hit, LOSDistance, ObstacleMask))
        {
            Debug.DrawLine(Origin, hit.point, Color.red);
            Debug.Log("Hit: " + hit.transform.name);
            if(hit.transform == Player)
            {
                Debug.Log("Player seen");
                LastSawPlayer = Time.time;
                _state = MonsterState.Chasing;
            }
        }
    }

    private void Update()
    {
        //Debug.Log($"Monster Current State: {_state}");

        DistanceFromPlayer = Vector3.Distance(m_Agent.transform.position, Player.position);

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
        }

        // if see player: set _state to chase, make player target, timeout chase if haven't seen player for a duration of time 
        LOSCheck(DistanceFromPlayer);

        if (Vector3.Distance(transform.position, Target.position) < 0.5f)
        {
            Debug.Log("Target reached");
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

        m_Animator.SetFloat("Speed", m_Agent.velocity.magnitude);
    }

    public void RespondToSound(Sound sound)
    {
        // Debug.Log("I HEARD THAT!!!!");

        _state = MonsterState.Investigating;

        if (sound.priority > TargetPriority)
        {
            Target.position = sound.pos;
            Debug.Log("Target to sound set");
        }
        
    }

// --- Patrolling ---
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
    private void Investigate()
    {
        // Debug.Log("Investigate function called");
        if (Target == null) 
        { 
            Debug.LogError("Monster Target is NULL"); 
            return; 
        }

        // Set speed

        if (ReachedTarget)
        {
            // survey growing area nearby

            
        }

    }

    // --- Chase ---
    private void Chase()
    {
        Debug.Log("Chase called");
        
        if((Time.time - LastSawPlayer) < ChaseTimeout)
        {
            _state = MonsterState.Investigating;
            return;
        }

        Target.position = Player.position;
    }
}
