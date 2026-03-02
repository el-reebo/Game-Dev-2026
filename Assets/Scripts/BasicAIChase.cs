using UnityEngine;
using UnityEngine.AI;

public class BasicAIChase : MonoBehaviour
{
    public Transform Target;
    public float AttackDistance;
    private NavMeshAgent m_Agent;
    private Animator m_Animator;
    private float m_Distance;

    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();
    }

    void Update()
    {
        m_Distance = Vector3.Distance(m_Agent.transform.position, Target.position);
        if (m_Distance < AttackDistance)
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

        // Set update speed value in animator
        m_Animator.SetFloat("Speed", m_Agent.velocity.magnitude);
    }

    //void OnAnimatorMove()
    //{
    //    if (m_Animator.GetBool("Attack") == false)
    //    {
    //        m_Agent.speed = (m_Animator.deltaPosition / Time.deltaTime).magnitude;
    //        Debug.Log(m_Agent.pathStatus);
    //    }
    //}
}
