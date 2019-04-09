using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Character))]
[RequireComponent(typeof(Stats))]
public class PathAI : MonoBehaviour
{
    [SerializeField]
    private Vector3 m_target;
    private NavMeshAgent m_agent;
    private Stats m_stats;
    private Character m_character;

    // Use this for initialization
    void Start()
    {
        m_agent = GetComponent<NavMeshAgent>();
        m_stats = GetComponent<Stats>();
        m_character = GetComponentInChildren<Character>();
        m_target = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //follow target
        if (m_target != transform.position)
        {
            m_agent.SetDestination(m_target);
            m_agent.speed = m_stats.speed;
        }

        if (m_agent.remainingDistance > m_agent.stoppingDistance)
            m_character.Move(m_agent.desiredVelocity, false, false);
        else
            m_character.Move(Vector3.zero, false, false);
    }

    public void setTarget(Vector3 target)
    {
        m_target = target;
    }
}

