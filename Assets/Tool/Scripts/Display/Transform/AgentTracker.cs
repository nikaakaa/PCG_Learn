using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentTracker : MonoBehaviour
{
    public Transform target;

    private NavMeshAgent agent;

    private Vector3 targetPos;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        targetPos = new Vector3(target.position.x, transform.position.y, target.position.z);
        agent.SetDestination(target.position);
    }
}
