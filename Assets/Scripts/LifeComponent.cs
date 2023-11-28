using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeComponent : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;


    private string type = "unknown";
    private GameObject lastAttacker;

    void Start()
    {
        if(maxHealth == 0f)
        {
            maxHealth = 100f;
        }
        currentHealth = maxHealth;
        SetObjectType();
    }

    public void TakeDamage(int damage, GameObject damageDealer)
    {
        currentHealth -= damage;

        lastAttacker = damageDealer;

        if(currentHealth <= 0)
        {
            switch (type)
            {
                //Player
                case "player":
                    break;
                //Normal Minion
                case "minion":
                    GameManagerScript.Instance.OnMinionDeath(lastAttacker, gameObject);
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
        if (gameObject.GetComponent<PlayerObj>() != null) type = "player";
        if (gameObject.GetComponent<MinionAI>() != null)
        {
            MinionType minionType = gameObject.GetComponent<MinionAI>().GetMinionType();
            if (minionType == MinionType.Minion) type = "minion";
            if (minionType == MinionType.PlayerMinion) type = "playerMinion";
        }
    }

    /*
    void Die()
    {
        GameManagerScript.Instance.OnMinionDeath(transform.position);
        Destroy(gameObject);
    }
    */
}
