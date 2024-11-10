using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Unity.Netcode;
using Unity.VisualScripting;

public class ListManager : NetworkBehaviour
{
    private GameManagerScript _gameManager;

    private List<GameObject> players = new List<GameObject>();
    private List<GameObject> minions = new List<GameObject>();

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void Update()
    {
        CheckPlayerListForNullValues();
    }


    private void CheckPlayerListForNullValues()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == null) players.RemoveAt(i);
        }
    }

    public void InitGameManager(GameManagerScript gameManager)
    {
        _gameManager = gameManager;
    }


    public void AddPlayerToList(GameObject player)
    {
        if(player != null)
        {
            players.Add(player);
        } else
        {
            DebugManager.LogError("Error: Cannot Add null as Player to the PlayerList");
        }
    }

    public void AddMinionToList(GameObject minion)
    {
        if(minion != null)
        {
            minions.Add(minion);
        }
    }

    public GameObject GetNextPlayerToPosition(Vector3 minionPosition)
    {
        int index = -1;
        float nearestPosition = float.MaxValue;  // Use MaxValue for initial comparison
        bool nearestPlayerFound = false;

        for (int i = 0; i < players.Count; i++)
        {
            // Ensure the player object is not null and its transform is also valid
            if (players[i] != null && players[i].transform != null)
            {
                float distance = Vector3.Distance(players[i].transform.position, minionPosition);
                if (distance < nearestPosition)
                {
                    nearestPosition = distance;
                    index = i;
                    nearestPlayerFound = true;
                }
            }
        }

        if (nearestPlayerFound && index != -1 && players[index] != null)
        {
            return players[index];
        }

        return null;
    }


    public GameObject GetNextMinionToPosition(Vector3 pos)
    {
        GameObject nextMinion = null;
        if(minions.Count > 0)
        {
            //return minions.OrderBy(minion => Vector3.Distance(minion.transform.position, pos)).First();
            float distance = 1000f;
            minions.ForEach(minion => { 
                if(minion != null)
                {
                    float nextDistance = Vector3.Distance(pos, minion.transform.position);
                    if (nextDistance < distance)
                    {
                        distance = nextDistance;
                        nextMinion = minion;
                    }
                }
            });
        }
        return nextMinion;
    }

    public List<GameObject> GetPlayersList()
    {
        return players;
    }
    

    public int GetPlayersCount()
    {
        return players.Count;
    }

    public void AddMinion(GameObject minion)
    {
        minions.Add(minion);
    }

    public void RemoveMinionFromList(GameObject minion)
    {
        if (!IsServer) return;
        if (minion)
        {
            int minionID = minion.GetComponent<MinionCore>().GetID();

            //int minionIndex = minions.FindIndex(minion => minion.GetComponent<MinionCore>().GetID() == minionID);
            minions.Remove(minion);
            NetworkObject minionObject = minion.GetComponent<NetworkObject>();
            if (minionObject != null)
                minionObject.Despawn();
        }
    }

    // Removing a player from the list will only accures when the player disconnects 
    // When a player is removed, we should also remove all his minions and destroy them
    public void RemovePlayerFromList(GameObject player)
    {
        if (!IsServer) return;
        if (player)
        {
            players.Remove(player);

            PlayerCore playerCore = player.GetComponent<PlayerCore>();
            if (playerCore)
            {
                // Destroy all the minions of the player object
                playerCore.GetMinions().ForEach(minion => {
                    NetworkObject minionObject = minion.GetComponent<NetworkObject>();
                    if (minionObject != null)
                        minionObject.Despawn();
                });
            }
            Debug.Log("Removed Player Successfully !");
        }
    }

    public GameObject GetPlayerWithID(int id)
    {
       if(players.Count > 0)
        {
            GameObject player = players.Find(player => player.GetComponent<PlayerCore>().GetID() == id);
            return player;
        }
        return null;
    }

    public void DestroyAllPlayerMinions(GameObject player)
    {
        if (!IsServer) return;
        if (player)
        {
            PlayerCore playerCore = player.GetComponent<PlayerCore>();
            if (playerCore)
            {
                playerCore.GetMinions().ForEach(minion => {
                    _gameManager.SpawnExplosion(minion.transform.position);
                    NetworkObject minionObject = minion.GetComponent<NetworkObject>();
                    if (minionObject != null)
                        minionObject.Despawn();
                });
                playerCore.EmptyMinionsList();
            }
        }
    }

    public int GetMinionsCount()
    {
        return minions.Count;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                RemovePlayerFromList(players[i]);
                DebugManager.Log("Removing PlayerObject due to PlayerDisconnect");
                break;
            }
        }
    }

}
