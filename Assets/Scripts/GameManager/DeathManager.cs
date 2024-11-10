using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DeathManager : NetworkBehaviour
{

    private GameManagerScript _gameManager;

    public void InitGameManager(GameManagerScript gameManager)
    {
        _gameManager = gameManager;
    }

    public void OnMinionEnemyDeath(GameObject lastAttacker, GameObject minion)
    {
        if (!IsServer ||lastAttacker == null || minion == null) return;

        // Remove Minion from the List
        _gameManager.RemoveMinionFromList(minion);

        //Spawn Explosion
        _gameManager.SpawnExplosion(minion.transform.position);

        // Get the ID of the Player who killed the Minion
        int killerID;
        // If it was a player minion who killed them minion
        if (lastAttacker.GetComponent<MinionPlayerAI>())
        {
            killerID = lastAttacker.GetComponent<MinionPlayerAI>().GetControllingPlayer().GetComponent<PlayerCore>().GetID();
        }
        else
        {
            killerID = lastAttacker.GetComponent<PlayerCore>().GetID();
        }

        //Spawn a new Player Minion
        PlayerCore pcoree = lastAttacker.GetComponent<PlayerCore>();
        if(pcoree && pcoree.GetMinions().Count < 1000)
        {
            _gameManager.SpawnNewPlayerMinion(killerID);
        }
    }

    public void OnPlayerMinionDeath(GameObject minion)
    {
        if(!IsServer) return;
        //Get the controlling player
        
        GameObject controllingPlayer = minion.GetComponent<MinionPlayerAI>().GetControllingPlayer();

        if (controllingPlayer)
        {
            List<GameObject> minionsList = controllingPlayer.GetComponent<PlayerCore>().GetMinions();

            if (minionsList.Count > 0)
            {
                if (minion != null)
                {
                    //int minionIndex = minionsList.FindIndex(minion => minion.GetComponent<MinionAI>().GetID() == minionID);
                    //minionsList.RemoveAt(minionIndex);
                    minionsList.Remove(minion);
                    _gameManager.SpawnExplosion(minion.transform.position);
                    NetworkObject minionObject = minion.GetComponent<NetworkObject>();
                    if (minionObject != null)
                        minionObject.Despawn();
                }
            }
        }
    }

    public void OnPlayerDeath(GameObject lastAttacker, GameObject deadPlayer)
    {
        if (lastAttacker == null || deadPlayer == null) return;
        // Spawn Explosion
        _gameManager.SpawnPlayerExplosion(deadPlayer.transform.position);

        // Destroy all player minions
        _gameManager.DestroyAllPlayerMinions(deadPlayer);
        // Move the player to a random position on the map

        /*
        float randomX = Random.Range(-100f, 100f);
        float randomZ = Random.Range(-100f, 100f);
        float positionY = deadPlayer.transform.position.y;
        deadPlayer.transform.position = new Vector3(randomX, positionY, randomZ);
        */

        LifeComponent deadPlayerLife = deadPlayer.GetComponent<LifeComponent>();
        if (deadPlayerLife)
        {
            deadPlayerLife.SetHealth(100f);
        }
    }

}
