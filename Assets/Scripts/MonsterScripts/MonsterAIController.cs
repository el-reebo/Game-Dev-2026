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
    public Transform PlayerPosition;

    [Header("Settings")]
    public float AttackDistance;

    [Header("Navigation Settings")]
    [SerializeField] private float NavMaxRadius = 10f;
    [SerializeField] private float NavMinRadius = 0.5f;
    [SerializeField] private float NavAngle = 45f;
    [SerializeField] private float MinEdgeDistance = 0f;
    [SerializeField] private float PatrolTargetTimeout = 4f; // Time until patrol target dismissed

    [Header("Patrol Settings")]
    [SerializeField] private float MinStationaryTime = 0.5f;
    [SerializeField] private float MaxStationaryTime = 10f;
    
    private NavMeshAgent m_Agent;
    private Animator m_Animator;

    private Transform Target;
    private float DistanceFromPlayer;
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

    private void Update()
    {
        //Debug.Log($"Monster Current State: {_state}");

        DistanceFromPlayer = Vector3.Distance(m_Agent.transform.position, PlayerPosition.position);

        switch(_state)
        {
            case MonsterState.Patrolling:
                Patrol();
                break;
        }

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

    public void RespondToSound(Sound sound)
    {
        print("Sound heard");
    }
}
