using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileCore : NetworkBehaviour
{
    public float moveSpeed = 35.0f;
    public float life = 2.0f;
    public float _spawnTime;

    public float _damage = 100f;

    private Vector3 targetPosition;
    private Transform parent;

    private bool forceApplied = false;

    private GameObject damageDealer;

    private NetworkVariable<Color> _projectileColor = new(Color.black);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _projectileColor.OnValueChanged += OnChangecolor;
        //Rigidbody rb = GetComponent<Rigidbody>();
        //rb.velocity = transform.forward * moveSpeed
    }

    private void Start()
    {
        ChangeColor();
        Move();
        _spawnTime = Time.deltaTime;
    }

    void Update()
    {
        Move();
    }

    private void DestroyIfLifeExceeded()
    {
        if (!IsServer) return;
        if(_spawnTime - Time.deltaTime > life)
        {
            NetworkObject projectileNetworkObject = GetComponent<NetworkObject>();
            if(projectileNetworkObject != null)
                projectileNetworkObject.Despawn();
        }
    }

    private void Move()
    {
        if (parent != null && !forceApplied)
        {
            Vector3 direction = parent.transform.forward;
            direction.y = 0;
            transform.forward = direction.normalized;
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = transform.forward * moveSpeed;
            forceApplied = true;
        }
    }

    public void SetDamageDealer(GameObject obj)
    {
        damageDealer = obj;
    }

    public void SetForward(Transform shooter)
    {
        parent = shooter;
    }

    private void ChangeColor()
    {
        if (!IsServer) return;
        if(damageDealer != null)
        {
            PlayerCore PlayerCore = damageDealer.GetComponent<PlayerCore>();
            if(PlayerCore)
            {
                Color newColor = PlayerCore.GetPlayerColor();
                _projectileColor.Value = newColor;
            }
        }
    }

    private void OnChangecolor(Color oldColor, Color newColor)
    {
        UpdateProjectileColorOnClients(newColor);
    }


    private void UpdateProjectileColorOnClients(Color newColor)
    {
        if(!IsClient) return;
        Renderer rend = GetComponent<Renderer>();
        if (rend)
        {
            rend.material.color = newColor;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (damageDealer == null) return;

        NetworkObject projectileNetworkObject = gameObject.GetComponent<NetworkObject>();
       
        if (projectileNetworkObject == null || !projectileNetworkObject.IsSpawned)
        {
            return;
        }

        if (other.CompareTag("Obstacle"))
        {
            GameManagerScript.Instance.SpawnMinionAttack(transform.position);
            if (IsServer)
            {
                projectileNetworkObject.Despawn();
            }
        }
        else if (other.CompareTag("Player") && other.gameObject != damageDealer)
        {
            dealDamage(other.gameObject, (_damage / 3f));
            GameManagerScript.Instance.SpawnMinionAttack(transform.position);
            if (IsServer)
            {
                projectileNetworkObject.Despawn();
            }
        }
        else if (other.CompareTag("Minion"))
        {
            LifeComponent minionLife = other.GetComponent<LifeComponent>();
            MinionCore minionAi = other.GetComponent<MinionCore>();

            if (minionAi.GetMinionType() == MinionType.Minion)
            {
                minionLife.TakeDamage(_damage, damageDealer);
                GameManagerScript.Instance.SpawnMinionAttack(transform.position);
                if (IsServer)
                {
                    projectileNetworkObject.Despawn();
                }
            }
            else if (minionAi.GetMinionType() == MinionType.PlayerMinion)
            {
                MinionPlayerAI playerMinionAI = other.GetComponent<MinionPlayerAI>();
                int damageDealerID = damageDealer.GetComponent<PlayerCore>().GetID();
                int controllingPlayerID = playerMinionAI.GetControllingPlayer().GetComponent<PlayerCore>().GetID();

                if (damageDealerID != controllingPlayerID)
                {
                    minionLife.TakeDamage(_damage, damageDealer);
                    GameManagerScript.Instance.SpawnMinionAttack(transform.position);
                    if (IsServer)
                    {
                        projectileNetworkObject.Despawn();
                    }
                }
            }
        }
    }


    private void dealDamage(GameObject other, float damage)
    {
        LifeComponent otherLife = other.GetComponent<LifeComponent>();
        if (otherLife)
        {
            otherLife.TakeDamage(damage, gameObject);
        }
    }
}
