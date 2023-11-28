using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class GameManagerScript : NetworkBehaviour
{
    public static GameManagerScript Instance;
    //Expolisions
    public GameObject explosionPrefab1;
    public GameObject minionAttackPrefab1;

    //Players
    public GameObject playerPrefab;
    public GameObject player;
    public GameObject playerAIPrefab;

    //Minions
    public GameObject minionPrefab;
    public GameObject playerMinionPrefab;

    //Projectiles
    public GameObject projectile_1;

    private List<GameObject> players = new List<GameObject>();
    private List<GameObject> minions = new List<GameObject>();

    //Materials
    public Material mat1;
    public Material mat2;
    public Material mat3;
    public Material mat4;
    public Material mat5;
    public Material mat6;
    public Material mat7;
    public Material mat8;

    private List<Material> materials = new List<Material>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            initMaterials();
            //initAI();
            initMinions();
            //players.Add(InstatitatePlayer(playerPrefab, new Vector3(0f, 3.5f, 0f)));
        }
        else
        {
            // Ensure there's only one instance of the GameManager
            Destroy(this.gameObject);
        }

        //InvokeRepeating("LogInfo", 1f, 1f);
    }

    void Update()
    {
        if(minions.Count <= 5)
        {
            initMinions();
        }

    }

    public void initMaterials()
    {
        materials.Add(mat1);
        materials.Add(mat2);
        materials.Add(mat3);
        materials.Add(mat4);
        materials.Add(mat5);
        materials.Add(mat6);
        materials.Add(mat7);
        materials.Add(mat8);
    }

    public Material SetPlayerMaterial()
    {
        int random = Random.Range(0, materials.Count - 1);
        Material mat = materials[random];
        materials.RemoveAt(random);
        return mat;
    }

    public Color GetColor()
    {
        int random = Random.Range(0, materials.Count - 1);
        Color color = materials[random].color;
        materials.RemoveAt(random);
        return color;
    }

    public void AddPlayer(GameObject createdPlayer)
    {
        if (!isServer) return;
        if(createdPlayer != null)
        {
            int randomID = Random.Range(100, 999);
            createdPlayer.GetComponent<PlayerObj>().SetID(randomID);
            Color oldColor = createdPlayer.transform.Find("Body").GetComponent<Renderer>().material.color;
            Color newColor = GetColor();
            createdPlayer.GetComponent<PlayerObj>().SetPlayerColor(newColor);
            createdPlayer.GetComponent<PlayerObj>().SetColor(oldColor, newColor);
            players.Add(createdPlayer);
            Debug.Log("Successfully created a player");
        }
    }


    public Transform GetPlayerTransform()
    {
        if (player != null)
        {
            return player.transform;
        }
        return null;

    }

    public GameObject GetProjectile1()
    {
        return projectile_1;
    }

    private GameObject InstatitatePlayer(GameObject playerPrefab, Vector3 position)
    {
        GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
        int id = Random.Range(100, 999);
        player.GetComponent<PlayerObj>().SetID(id);
        Debug.Log("Successfully instatitated player with the ID:" + id);
        return player;
    }

    public GameObject GetNextTargetNormal(Vector3 minionPosition)
    {
        if (players.Count <= 0) return null;
        GameObject nextTarget = players.OrderBy(player => Vector3.Distance(player.transform.position, minionPosition)).First();
        return nextTarget;
    }

    //Returns the closest normal minion relative to own position
    public GameObject GetNextMinion(Vector3 pos)
    {
        return minions.OrderBy(minion => Vector3.Distance(pos, minion.transform.position)).First();
    }

    public GameObject GetNextPlayer(Vector3 pos, int controllingPlayerID)
    {
        IEnumerable<GameObject> filteredPlayers = players.Where(player => player.GetComponent<PlayerObj>().GetID() != controllingPlayerID);
        if(filteredPlayers.Count() >= 1)
        {
            return filteredPlayers.OrderBy(player => Vector3.Distance(pos, player.transform.position)).First();
        }
        return null;
    }

    //Returns the next Target player Minion
    public GameObject GetNextPlayerMinion(Vector3 pos, GameObject playerToAttack)
    {
        List<GameObject> playerMinions = playerToAttack.GetComponent<PlayerObj>().GetMinions();

        if(playerMinions.Count > 0)
        {
            return playerMinions.OrderBy(minion => Vector3.Distance(pos, minion.transform.position)).First();
        }
        return null;
    }

    //Instantitating
    void initAI()
    {
        if(playerAIPrefab != null)
        {
            players.Add(Instantiate(playerAIPrefab, new Vector3(20f, 3.25f, 20f), Quaternion.identity));
            players.Add(Instantiate(playerAIPrefab, new Vector3(1f, 3.25f, 1f), Quaternion.identity));
            players.Add(Instantiate(playerAIPrefab, new Vector3(30f, 3.25f, 30f), Quaternion.identity));
        }
        players.ForEach(player => {
            int id = Random.Range(1000, 9999);
            PlayerObj playerObj = player.GetComponent<PlayerObj>();
            /*
             
            Hier Material !!!!
             
             */
            playerObj.SetID(id);
            playerObj.SetTypeAI();

        });
    }

    void SpawnMinion(float x, float y, float z)
    {
        if(minionPrefab != null)
        {
            GameObject minion = Instantiate(minionPrefab, new Vector3(x,y,z), Quaternion.identity);
            minions.Add(minion);
            NetworkServer.Spawn(minion);
        }
    }

    void initMinions()
    {
        if (!isServer) return;
        if(minionPrefab != null)
        {
            //in the middle
            /*
            minions.Add(Instantiate(minionPrefab, new Vector3(1f, 3.25f, 1f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(2f, 3.25f, 2f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(3f, 3.25f, 3f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(4f, 3.25f, 4f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(5f, 3.25f, 5f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-5f, 3.25f, -1f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-4f, 3.25f, -2f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-3f, 3.25f, -3f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-2f, 3.25f, -4f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-1f, 3.25f, -5f), Quaternion.identity));
            */
            SpawnMinion(1f, 3.25f, 1f);
            SpawnMinion(2f, 3.25f, 2f);
            SpawnMinion(3f, 3.25f, 3f);
            SpawnMinion(4f, 3.25f, 4f);
            SpawnMinion(5f, 3.25f, 5f);
            SpawnMinion(-5f, 3.25f, -1f);
            SpawnMinion(-4f, 3.25f, -2f);
            SpawnMinion(-3f, 3.25f, -3f);
            SpawnMinion(-2f, 3.25f, -4f);
            SpawnMinion(-1f, 3.25f, -5f);
            /*
            //Top Left Corner
            minions.Add(Instantiate(minionPrefab, new Vector3(-86f, 3.25f, 85f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-87f, 3.25f, 87f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-88f, 3.25f, 88f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-89f, 3.25f, 89f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-90f, 3.25f, 90f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-91f, 3.25f, 91f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-92f, 3.25f, 92f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-93f, 3.25f, 93f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-94f, 3.25f, 94f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-95f, 3.25f, 95f), Quaternion.identity));

            //Top Right Corner
            minions.Add(Instantiate(minionPrefab, new Vector3(86f, 3.25f, 85f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(87f, 3.25f, 87f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(88f, 3.25f, 88f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(89f, 3.25f, 89f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(90f, 3.25f, 90f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(91f, 3.25f, 91f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(92f, 3.25f, 92f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(93f, 3.25f, 93f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(94f, 3.25f, 94f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(95f, 3.25f, 95f), Quaternion.identity));

            //Bottom Right Corner
            minions.Add(Instantiate(minionPrefab, new Vector3(86f, 3.25f, -85f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(87f, 3.25f, -87f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(88f, 3.25f, -88f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(89f, 3.25f, -89f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(90f, 3.25f, -90f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(91f, 3.25f, -91f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(92f, 3.25f, -92f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(93f, 3.25f, -93f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(94f, 3.25f, -94f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(95f, 3.25f, -95f), Quaternion.identity));

            //Bottom Left Corner
            minions.Add(Instantiate(minionPrefab, new Vector3(-86f, 3.25f, -85f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-87f, 3.25f, -87f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-88f, 3.25f, -88f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-89f, 3.25f, -89f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-90f, 3.25f, -90f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-91f, 3.25f, -91f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-92f, 3.25f, -92f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-93f, 3.25f, -93f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-94f, 3.25f, -94f), Quaternion.identity));
            minions.Add(Instantiate(minionPrefab, new Vector3(-95f, 3.25f, -95f), Quaternion.identity));
            */
            //Set IDS for the minions
            minions.ForEach(minion =>
            {
                int randomID = Random.Range(100000, 999999);
                minion.GetComponent<MinionAI>().SetID(randomID);
            });
        }
    }
    // //Instantitating

    //OnDeathMethods
    public void OnMinionDeath(GameObject lastAttacker, GameObject minion)
    {
        if (!isServer) return;
        //Destroy and Remove the minion from the list
        int minionID = minion.GetComponent<MinionAI>().GetID();
        int minionIndex = minions.FindIndex(minion => minion.GetComponent<MinionAI>().GetID() == minionID);
        if(minionIndex < minions.Count && minionIndex >= 0)
        {
            minions.RemoveAt(minionIndex);
            Destroy(minion);
        }
        //Find who killed the minion
        int killerID = 0;
        //Minion Killed by playerminion
        if (lastAttacker.GetComponent<MinionAI>() != null)
        {
            killerID = lastAttacker.GetComponent<MinionAI>().GetControllingPlayer().GetComponent<PlayerObj>().GetID();
        }
        else
        {
            killerID = lastAttacker.GetComponent<PlayerObj>().GetID();
        }

        int playerIndex = players.FindIndex(player => player.GetComponent<PlayerObj>().GetID() == killerID);
        GameObject player = players[playerIndex];

        //Instantitate a new playerMinion
        float randomOffset = Random.Range(4f, 6f);
        Vector3 instancePosition = player.transform.position;
        GameObject newPlayerMinion = Instantiate(playerMinionPrefab, instancePosition, Quaternion.identity);

        //Set The color of the minion to match the player
        newPlayerMinion.GetComponent<Renderer>().material = player.transform.Find("Body").GetComponent<Renderer>().material;
        //Change the Type of the spawned Minion
        newPlayerMinion.GetComponent<MinionAI>().SetMinionType(MinionType.PlayerMinion);
        newPlayerMinion.GetComponent<MinionAI>().SetControllingPlayer(player);
        int randomID = Random.Range(100, 999);
        newPlayerMinion.GetComponent<MinionAI>().SetID(randomID);
        //Add the newPlayerMinion to the list
        player.GetComponent<PlayerObj>().GetMinions().Add(newPlayerMinion);


        Color playerColor = player.transform.Find("Body").GetComponent<Renderer>().material.color;
        NetworkServer.Spawn(newPlayerMinion);
        newPlayerMinion.GetComponent<MinionAI>().SetMinionColor(playerColor);
        newPlayerMinion.GetComponent<MinionAI>().SetColor(newPlayerMinion.GetComponent<Renderer>().material.color, playerColor);
        //Spawn the Explosion Effect

        SpawnExplosion(minion.transform.position);
    }

    public void OnPlayerMinionDeath(GameObject minion)
    {
        //Get the controlling player
        GameObject controllingPlayer = minion.GetComponent<MinionAI>().GetControllingPlayer();
        int minionID = minion.GetComponent<MinionAI>().GetID();

        List<GameObject> minionsList = controllingPlayer.GetComponent<PlayerObj>().GetMinions();

        if (minionsList.Count > 0)
        {
            if (minion != null)
            {
                int minionIndex = minionsList.FindIndex(minion => minion.GetComponent<MinionAI>().GetID() == minionID);
                minionsList.RemoveAt(minionIndex);
                SpawnExplosion(minion.transform.position);
                Destroy(minion);
            }

        }
    }
    // //OnDeath Methods


    //Visual Effects
    void SpawnExplosion(Vector3 pos)
    {
        if (explosionPrefab1 != null)
        {
            //Quaternion represents a rotation with no rotation applied
            GameObject explosion = Instantiate(explosionPrefab1, pos, Quaternion.identity);
            NetworkServer.Spawn(explosion);
        }
    }

    public void SpawnMinionAttack(Vector3 pos)
    {
        
        if (minionAttackPrefab1 != null)
        {
            //Quaternion represents a rotation with no rotation applied
            GameObject minionAttack = Instantiate(minionAttackPrefab1, pos, Quaternion.identity);
            NetworkServer.Spawn(minionAttack);
        }
        
    }

    ////Visual Effects

    private void LogInfo()
    {
        
        players.ForEach(player => {
            Debug.Log("Player with ID: " + player.GetComponent<PlayerObj>().GetID() + " Has " + player.GetComponent<PlayerObj>().GetMinions().Count + " Minions Left");
        });

        Debug.Log("Normal Minions in Game: " + minions.Count);
    }


}