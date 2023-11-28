using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public enum MinionType
{
    Minion,
    PlayerMinion
}
public class MinionAI : NetworkBehaviour
{
    //Type and ID
    int ID = 999;
    MinionType minionType = MinionType.Minion;

    //Range and Speed
    public float visionRange = 10f;
    public float playerRange = 8f;
    public float attackRange = 1f;
    public float moveSpeed = 10f;
    public float damage = 10f;
    public Vector3 spawnPosition;

    //Attack
    private float lastAttackTime = 0f;
    private float attackCooldown = 1f;

    //Objects
    private GameObject nextTarget;
    private GameObject controllingPlayer;

    [SyncVar(hook = nameof(SetColor))]
    Color minionColor = Color.black;

    Material cachedMaterial;

    // Start is called before the first frame update
    void Start()
    {
        spawnPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        switch (minionType)
        {
            case MinionType.Minion:
                NormalMinionBehaviour();
                StartAI(nextTarget, spawnPosition, 20);
                break;
            case MinionType.PlayerMinion:
                PlayerMinionBehaviour();
                break;
        }
    }

    public Color GetMinionColor()
    {
        return minionColor;
    }

    public void SetMinionColor(Color color)
    {
        minionColor = color;
    }

    public void SetColor(Color oldColor, Color newColor)
    {
        if (cachedMaterial == null)
            cachedMaterial = GetComponent<Renderer>().material;
        cachedMaterial.color = newColor;
        Debug.Log("New Material is Working ! Color: " + newColor);
    }

    private void OnDestroy()
    {
        Destroy(cachedMaterial);
    }


    void NormalMinionBehaviour()
    {
        nextTarget = GameManagerScript.Instance.GetNextTargetNormal(transform.position);
    }

    void PlayerMinionBehaviour()
    {
        //Check if there is controlling player
        if (controllingPlayer == null) return;

        //Maintain Distance to controlling Player
        int controllingPlayerID = controllingPlayer.GetComponent<PlayerObj>().GetID();

        float distanceToPlayer = Vector3.Distance(transform.position, controllingPlayer.transform.position);

        if (distanceToPlayer > playerRange) MoveToPlayer();

        //If Still nearby the controlling player check the next target
        else if (distanceToPlayer <= visionRange)
        {
            //Get nearest Player and Nearest minion
            GameObject nextPlayer = GameManagerScript.Instance.GetNextPlayer(transform.position, controllingPlayerID);
            GameObject nextMinion = GameManagerScript.Instance.GetNextMinion(transform.position);

            float distanceToTargetPlayer = 1000f;
            float distanceToTargetMinion = 1000f;

            if (nextPlayer != null) distanceToTargetPlayer = Vector3.Distance(transform.position, nextPlayer.transform.position);
            if (nextMinion != null) distanceToTargetMinion = Vector3.Distance(transform.position, nextMinion.transform.position);

            //Attack Minion If Closer
            if (distanceToTargetMinion < distanceToTargetPlayer)
            {
                AttackTarget(nextMinion);
            }
            //Attack Player if Closer
            if (distanceToTargetPlayer < distanceToTargetMinion)
            {
                //AttackTarget(nextPlayer);
                
                List<GameObject> playerMinions = nextPlayer.GetComponent<PlayerObj>().GetMinions();
                //TargetPlayer has Minions
                if (playerMinions.Count > 0)
                {
                    //Get a random Minion to Attack
                    System.Random random = new System.Random();
                    int randomIndex = random.Next(playerMinions.Count - 1);
                    nextTarget = playerMinions[randomIndex];
                    if(nextTarget != null) AttackTarget(nextTarget);
                }
                //TargetPlayer Has No Minions
                else
                {
                    AttackTarget(nextPlayer);
                }
                
            }

        }
    }

    void AttackTarget(GameObject target)
    {
        if(target != null)
        {   
            Debug.DrawLine(transform.position, target.transform.position, Color.red);
            float distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
            if(distanceToTarget <= visionRange)
            {
                if (distanceToTarget <= attackRange)
                {
                    //Debug.DrawLine(transform.position, target.transform.position, Color.green);
                    if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        Vector3 midPosition = (transform.position + target.transform.position) / 2;
                        GameManagerScript.Instance.SpawnMinionAttack(midPosition);
                        //LifeComponent playerLife = GameManagerScript.Instance.GetPlayer().GetComponent<LifeComponent>();
                        //playerLife.TakeDamage(1, gameObject);
                        LifeComponent targetLife = target.GetComponent<LifeComponent>();
                        if (targetLife != null) targetLife.TakeDamage(10, gameObject);
                        lastAttackTime = Time.time;
                    }
                } else
                {
                    MoveToTarget(target);
                }
            }
        }
    }

    void StartAI(GameObject target, Vector3 position, float radius)
    {
        if(nextTarget != null)
        {

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

        if ((Vector3.Distance(transform.position, position) > radius))
        {
            GoBackToPosition(spawnPosition);
        }

        else if (distanceToTarget <= attackRange)
        {
            AttackTarget(nextTarget);
        }

        else if (distanceToTarget <= visionRange)
        {
            MoveToTarget(nextTarget);
        }

        else if ((distanceToTarget > visionRange))
        {
            GoBackToPosition(spawnPosition);
        }
        }
    }

    public void SetControllingPlayer(GameObject obj)
    {
        controllingPlayer = obj;
    }

    /*
    void MoveToTarget(GameObject target)
    {
        // Calculate the direction vector from the minion to the player.
        Vector3 directionToPlayer = (target.transform.position - transform.position).normalized;

        // Calculate a random offset within the specified range.
        //float randomOffset = Random.Range(0f, 3f);
        float randomOffset = 0f;

        // Calculate the target position with the random offset.
        Vector3 targetPosition = target.transform.position - directionToPlayer * randomOffset;

        // Move the minion towards the target position.
        transform.Translate((targetPosition - transform.position).normalized * moveSpeed * Time.deltaTime);
    }

    void MoveToPlayer()
    {
            float randomOffsetX = Random.Range(-10f, 10f);
            float randomOffsetZ = Random.Range(-10f, 10f);

            Vector3 randomOffset = new Vector3(randomOffsetX, 0f, randomOffsetZ);

            Vector3 targetPosition = controllingPlayer.transform.position + randomOffset;

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
    */

    void MoveToTarget(GameObject target)
    {
        // Calculate the direction vector from the minion to the target.
        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;

        // Calculate a random offset within the specified range.
        float randomOffset = 0f;

        // Calculate the target position with the random offset.
        Vector3 targetPosition = target.transform.position - directionToTarget * randomOffset;

        // Calculate the velocity needed to reach the target position.
        Vector3 velocity = (targetPosition - transform.position).normalized * moveSpeed;

        // Apply the velocity to the rigidbody.
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = velocity;
        // Check if the minion has reached the target position.
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            // If the distance is small, stop the minion.
            rb.velocity = Vector3.zero;
        }
    }

    void MoveToPlayer()
    {
        /*
        float randomOffsetX = Random.Range(-10f, 10f);
        float randomOffsetZ = Random.Range(-10f, 10f);

        Vector3 randomOffset = new Vector3(randomOffsetX, 0f, randomOffsetZ);

        Vector3 targetPosition = controllingPlayer.transform.position + randomOffset;
        */
        // Calculate the velocity needed to reach the target position.
        Vector3 velocity = (controllingPlayer.transform.position - transform.position).normalized * moveSpeed;

        // Apply the velocity to the rigidbody.
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = velocity;
        // Check if the minion has reached the target position.
        if (Vector3.Distance(transform.position, controllingPlayer.transform.position) < 0.1f)
        {
            // If the distance is small, stop the minion.
            rb.velocity = Vector3.zero;
        }
    }

    void MoveRandomly()
    {

    }

    void GoBackToPosition(Vector3 position)
    {
        transform.Translate((spawnPosition - transform.position).normalized * moveSpeed * Time.deltaTime);
    }

    public void SetMinionType(MinionType typ)
    {
        minionType = typ;
    }

    private float GetDistanceToNextTarget()
    {
        return Vector3.Distance(nextTarget.transform.position, transform.position);
    }

    public GameObject GetControllingPlayer()
    {
        return controllingPlayer;
    }

    public MinionType GetMinionType()
    {
        return minionType;
    }

    public void SetID(int identification)
    {
        ID = identification;
    }

    public int GetID()
    {
        return ID;
    }

}
