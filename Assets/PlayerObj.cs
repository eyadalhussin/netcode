using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PlayerType
{
    player,
    ai
}

public class PlayerObj : NetworkBehaviour
{
    private int ID;
    private List<GameObject> minionsList = new List<GameObject>();
    PlayerType playerType;

    //Movement
    public float moveSpeed = 15f;

    //Attack
    public Transform projectileSpawnPos;

    private float visionRange = 10f;
    //private float attackRange = 5f;
    private float attackInterval = 0.2f;
    private float lastAttackTime;

    private GameObject nextTarget = null;

    [SyncVar(hook = nameof(SetColor))]
    Color playerColor = Color.black;

    Material cachedMaterial;

    void Start()
    {
        if (isLocalPlayer == false) return;
        if(playerType == PlayerType.ai)
        {
            lastAttackTime -= attackInterval;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(playerType == PlayerType.ai)
        {
            AIBehaviour();
        }
    }

    public void SetPlayerColor(Color newColor)
    {
        playerColor = newColor;
    }

    public void SetColor(Color oldColor, Color newColor)
    {
        if (cachedMaterial == null)
            cachedMaterial = transform.Find("Body").GetComponent<Renderer>().material;
        Debug.Log("Dein Material ist" + cachedMaterial);
        cachedMaterial.color = newColor;
    }

    private void OnDestroy()
    {
        Destroy(cachedMaterial);
    }


    private void AIBehaviour()
    {
        GameObject nextMinion = GameManagerScript.Instance.GetNextMinion(transform.position);
        GameObject nextPlayer = GameManagerScript.Instance.GetNextPlayer(transform.position, ID);

        float distanceToMinion = 1000f;
        float distanceToPlayer = 1000f;

        if (nextMinion != null) distanceToMinion = Vector3.Distance(nextMinion.transform.position, transform.position);
        if (nextPlayer != null) distanceToPlayer = Vector3.Distance(nextPlayer.transform.position, transform.position);

        GameObject target = null;

        if (distanceToMinion < distanceToPlayer) target = nextMinion;
        if (distanceToMinion > distanceToPlayer) target = nextPlayer;

        Debug.DrawLine(transform.position, target.transform.position, Color.green);

        float distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
        if (distanceToTarget > visionRange)
        {
            MoveToTarget(target);
        }
        if (distanceToTarget <= visionRange)
        {
            Attack(target);
        }
    }

    private void MoveToTarget(GameObject target)
    {
        transform.LookAt(target.transform);
        // Calculate the direction vector from the minion to the target.
        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;

        // Calculate the target position.
        Vector3 targetPosition = transform.position + directionToTarget * moveSpeed * Time.deltaTime;

        // Move the player towards the target position.
        transform.position = targetPosition;
    }


    private void Attack(GameObject nextTarget)
    {
        if (nextTarget != null)
        {
            if (Time.time - lastAttackTime >= attackInterval)
            {
                transform.LookAt(nextTarget.transform);
                GameObject projectilePrefab = GameManagerScript.Instance.GetProjectile1();
                //Instantitate the projectile
                GameObject bullet = Instantiate(projectilePrefab, projectileSpawnPos.position, Quaternion.identity);
                //Change the color of the projectile to match the shooter

                Material shooterMat = transform.Find("Body").GetComponent<Renderer>().material;
                bullet.GetComponent<Renderer>().material = shooterMat;
                //Adjust the forward position
                Lifetime lifetimeComponent = bullet.GetComponent<Lifetime>();
                lifetimeComponent.SetDamageDealer(gameObject);
                lifetimeComponent.SetForward(transform);
                lastAttackTime = Time.time; // Set the last attack time to the current time
            }
        }
    }

    public void SetID(int idNumber)
    {
        ID = idNumber;
    }

    public int GetID()
    {
        return ID;
    }

    public List<GameObject> GetMinions()
    {
        return minionsList;
    }

    public void SetTypeAI()
    {
        playerType = PlayerType.ai;
    }

}
