using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaypointPatrol : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public Transform[] WayPoints;

    private int m_CurrentWaypointIndex;

    void Start()
    {
        navMeshAgent.SetDestination(WayPoints[0].position);
    }

    void Update()
    {
        if(navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
        {
            m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % WayPoints.Length;
            navMeshAgent.SetDestination(WayPoints[m_CurrentWaypointIndex].position);
        }
    }
}
