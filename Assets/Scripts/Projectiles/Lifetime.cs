using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifetime : NetworkBehaviour
{
    public float moveSpeed = 5.0f;
    public float life = 2.0f;

    public int damage = 25;

    private Vector3 targetPosition;
    private Transform parent;

    private bool forceApplied = false;

    private GameObject damageDealer;

    // Update is called once per frame
    void Start()
    {
        SetBulletColor();
        Destroy(gameObject, life);
    }

    void Update()
    {
        if (isServer)
        {
            if (parent != null && !forceApplied)
            {
                transform.forward = parent.transform.forward;
                Rigidbody rb = GetComponent<Rigidbody>();
                rb.AddForce(transform.forward * moveSpeed);
                forceApplied = true;
                Debug.Log("Executing");
            }
        }
    }

    private void SetBulletColor()
    {
        GetComponent<Renderer>().material = damageDealer.transform.Find("Body").GetComponent<Renderer>().material;
    }

    public void SetDamageDealer(GameObject obj)
    {
        damageDealer = obj;
    }

    public void SetForward(Transform shooter)
    {
        parent = shooter;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
        if (other.CompareTag("Minion"))
        {
            LifeComponent minionLife = other.GetComponent<LifeComponent>();
            MinionAI minionAi = other.GetComponent<MinionAI>();
            //Normal Minion
            if(minionAi.GetMinionType() == MinionType.Minion)
            {
                minionLife.TakeDamage(damage, damageDealer);
                Destroy(gameObject);
            }

            //PlayerMinion
            if (minionAi.GetMinionType() == MinionType.PlayerMinion)
            {
                //Get the ID who shooted the projectile
                int damageDealerID = damageDealer.GetComponent<PlayerObj>().GetID();
                //Get the ID of the controlling player
                int controllingPlayerID = minionAi.GetControllingPlayer().GetComponent<PlayerObj>().GetID();
                //if the ID's does not match, another player should have attacked the minion
                if(damageDealerID != controllingPlayerID)
                {
                    minionLife.TakeDamage(damage, damageDealer);
                    Destroy(gameObject);
                }
            }
        }
    }
}
