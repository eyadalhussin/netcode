using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AINormalBehaviour : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;

    public float moveSpeed = 10f;
    public float attackRange = 40f;
    public float visualRange = 15f;

    public float attackCooldown = 0.5f;
    private float lastAttackTime;

    public Transform projectileSpawnPos;

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        lastAttackTime = -attackCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        //if no player in radius move randomly
        //Get Player position
        Transform playerTransform = GameManagerScript.Instance.GetPlayerTransform();

        //Calculate Distance
        float distance = Vector3.Distance(playerTransform.position, transform.position);

        if (distance <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            Debug.Log("Attacking Player !!");
            lastAttackTime = Time.time; // Set the last attack time to the current time
            Attack(playerTransform);
        }
    }
    //Move randomly
    void MoveRandomly()
    {
        float randomZ = Random.Range(-15f, 15f);
        float randomX = Random.Range(-15f, 15f);
        Vector3 destination = new Vector3(transform.position.x + randomX, 0f, transform.position.z + randomZ);
        navMeshAgent.SetDestination(destination);
    }
    /*
    void AttackPlayer(Transform playerTransform)
    {
        transform.LookAt(playerTransform);
        if(projectilePrefab != null)
        {
            GameObject bullet = Instantiate(projectilePrefab, projectileSpawnPos.position, Quaternion.identity);
            Lifetime lifetimeComponent = bullet.GetComponent<Lifetime>();
            lifetimeComponent.SetForward(transform);

        }
    }
    */
    void MoveToPlayer(Transform playerTransform)
    {
        navMeshAgent.SetDestination(playerTransform.position);
        transform.LookAt(playerTransform);
    }

    void Attack(Transform objectToAttack)
    {
        transform.LookAt(objectToAttack);
        //Get The prefab from the GameManager
        GameObject projectilePrefab = GameManagerScript.Instance.GetProjectile1();
        //Instantitate the projectile
        GameObject bullet = Instantiate(projectilePrefab, projectileSpawnPos.position, Quaternion.identity);
        //Change the color of the projectile to match the shooter
        Material shooterMat = GetComponent<Renderer>().material;
        bullet.GetComponent<Renderer>().material = shooterMat;
        //Adjust the forward position
        Lifetime lifetimeComponent = bullet.GetComponent<Lifetime>();
        lifetimeComponent.SetDamageDealer(gameObject);
        lifetimeComponent.SetForward(transform);
    }



}
