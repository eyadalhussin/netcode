using UnityEngine;
using Unity.Netcode;

public class LifeComponent : NetworkBehaviour
{
    private float maxHealth;
    private float currentHealth;


    private string type = "unknown";
    private GameObject lastAttacker;

    public override void OnNetworkSpawn()
    {
        if (maxHealth == 0f)
        {
            maxHealth = 100f;
        }
        currentHealth = maxHealth;
        SetObjectType();
    }

    public void TakeDamage(float damage, GameObject damageDealer)
    {
        PlayerCore playerCore = GetComponent<PlayerCore>();
        if (playerCore != null)
        {
            if (playerCore.GetMinions().Count > 3)
            {
                return;
            } else
            {
                currentHealth -= damage;
            }
        } else
        {
            currentHealth -= damage;
        }
        lastAttacker = damageDealer;
        if (currentHealth <= 0f)
        {
            switch (type)
            {
                //Player
                case "player":
                    GameManagerScript.Instance.OnPlayerDeath(lastAttacker, gameObject);
                    break;
                //Normal Minion
                case "minion":
                    GameManagerScript.Instance.OnMinionEnemyDeath(lastAttacker, gameObject);
                    break;
                //Player Minion
                case "playerMinion":
                    GameManagerScript.Instance.OnPlayerMinionDeath(gameObject);
                    break;
            }
        }
    }

    private void SetObjectType()
    {
        if (gameObject.GetComponent<PlayerCore>() != null) type = "player";
        if (gameObject.GetComponent<MinionCore>() != null)
        {
            MinionType minionType = gameObject.GetComponent<MinionCore>().GetMinionType();
            if (minionType == MinionType.Minion) type = "minion";
            if (minionType == MinionType.PlayerMinion) type = "playerMinion";
        }
    }

    public void SetHealth(float health)
    {
        if(health > maxHealth)
        {
            Debug.Log("Please set health smaller than 100!");
            return;
        }
        this.currentHealth = health;
    }

    public float GetCurrentHealth()
    {
        return this.currentHealth;
    }

    /*
    void Die()
    {
        GameManagerScript.Instance.OnMinionDeath(transform.position);
        Destroy(gameObject);
    }
    */
}
