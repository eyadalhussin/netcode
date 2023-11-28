using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class MinionController : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    public float radius = 15f;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
    void Update()
    {
        //Get the Player Position
        Vector3 playerPosition = GameManagerScript.Instance.GetPlayerTransform().position;
        float distance = Vector3.Distance(playerPosition, transform.position);

        if(distance <= radius)
        {
            navMeshAgent.SetDestination(playerPosition);
            transform.LookAt(GameManagerScript.Instance.GetPlayerTransform());
        }
    }
}
