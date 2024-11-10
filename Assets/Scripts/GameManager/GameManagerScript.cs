using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;


public class GameManagerScript : NetworkBehaviour
{
    public static GameManagerScript Instance;
    //Managers
    private SpawnManager _spawnManager;
    private DeathManager _deathManager;
    private ListManager _listManager;

    //Players
    public GameObject playerPrefab;
    public GameObject playerAIPrefab;

    //Minions
    public GameObject playerMinionPrefab;

    //Projectiles
    public GameObject projectile_1;

    //Colors
    List<Color> _playerColors = new List<Color>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // Ensure there's only one instance of the GameManager
            //if(IsServer) Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        StartTheGame();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    /*
    private void HandleSceneTransition(SceneEvent sceneEvent)
    {
        if(sceneEvent.SceneEventType == SceneEventType.LoadComplete && sceneEvent.SceneName == "Game")
        {
            if (IsServer)
            {
                StartTheGame();
            }
        }
    }*/

    /*
    This Method will replace Start(), since Start will be called at the very beginning in the "Offline" scene,
    only when changing the scene to the "Game", this method should be called*/
    public void StartTheGame()
    {
        DebugManager.Log("Starting the Game");
        InitManagers();
        InitColors();
        InitPlayers();

        if(_spawnManager)
        {
            _spawnManager.SetGameStarted();
            DebugManager.Log("Game Started");
            DebugManager.Log("Spawning Minions");
            _spawnManager.InitMinions();
            DebugManager.Log("Finished Spawning Minions");
        }
        //
    }

    private void InitPlayers()
    {
        if (IsServer)
        {
            foreach(var player in NetworkManager.Singleton.ConnectedClients)
            {
                InitPlayerObject(player);
            }
        }
    }

    private void InitPlayerObject(KeyValuePair<ulong, NetworkClient> player)
    {
        if (IsServer)
        {
            DebugManager.Log("Instantiating Palyer Object");
            if (playerPrefab != null)
            {
                DebugManager.Log("Playerprefab found");
                // Instantitating and Spawning on the Network
                GameObject playerInstance = Instantiate(playerPrefab, new Vector3(Random.Range(3f, 10f),0,Random.Range(3f, 10f)), Quaternion.identity);
                NetworkObject playerNetworkObject = playerInstance.GetComponent<NetworkObject>();
                playerNetworkObject.SpawnAsPlayerObject(player.Key, false);
                DebugManager.Log("Player spawned in the Network");

                // Setting Up the Player Color and .......
                PlayerCore playerCore = playerInstance.GetComponent<PlayerCore>();
                if (playerCore != null)
                {
                    playerCore.SetID((int)player.Key);
                    // Key is for ClientId , true is for DestroyWithScene
                    DebugManager.Log("Player got the Id: " + player.Key);
                    // Assign New color for the Player Object
                    Color color = _playerColors[_playerColors.Count - 1];
                    _playerColors.RemoveAt(_playerColors.Count - 1);
                    playerCore.SetPlayerColor(color);
                    DebugManager.Log("Player got the color: " + color);
                
                    _listManager.AddPlayerToList(playerInstance);
                    DebugManager.Log("Player added to the list in the Listmanager");
                }
            }
        }
    }


    //Initialize Managers because these are private at the start of the method
    private void InitManagers()
    {
        if (IsServer)
        {
            DebugManager.Log("Initializing Managers");
            //Spawn Manager
            _spawnManager = GetComponent<SpawnManager>();
            _spawnManager.InitGameManager(this);
            //Death Manager
            _deathManager = GetComponent<DeathManager>();
            _deathManager.InitGameManager(this);
            //List Manager
            _listManager = GetComponent<ListManager>();
            _listManager.InitGameManager(this);
            DebugManager.Log("Finished Initializing Managers");
        }
    }

    //Spawn Manager
    public void SpawnMinion(Vector3 spawnPosition)
    {
        if(_spawnManager != null)
        {
            _spawnManager.SpawnEnemyMinion(spawnPosition);
        }
    }
    public void SpawnExplosion(Vector3 explosionPosition)
    {
        if (_spawnManager != null)
        {
            _spawnManager.SpawnMinionDeathExplosion(explosionPosition);
        }
    }

    public void SpawnPlayerExplosion(Vector3 explosionPosition)
    {
        if (_spawnManager != null)
        {
            _spawnManager.SpawnPlayerExpolision(explosionPosition);
        }
    }

    public void SpawnMinionAttack(Vector3 attackPosition)
    {
        if (_spawnManager != null)
        {
            _spawnManager.SpawnMinionAttack(attackPosition);
        }
    }

    public void SpawnNewPlayerMinion(int killerID)
    {
        GameObject killer = _listManager.GetPlayerWithID(killerID);

        if(_spawnManager != null)
        {
            _spawnManager.SpawnNewPlayerMinion(killer);
        }

    }
    //Spawn Manager

    //Death Manager
    public void OnMinionEnemyDeath(GameObject lastAttacker, GameObject minion)
    {
        _deathManager.OnMinionEnemyDeath(lastAttacker, minion);
    }

    public void OnPlayerDeath(GameObject lastAttacker, GameObject deadplayer)
    {
        _deathManager.OnPlayerDeath(lastAttacker, deadplayer);
    }

    public void OnPlayerMinionDeath(GameObject minion)
    {
        _deathManager.OnPlayerMinionDeath(minion);
    }
    //Death Manager

    //List Manager


    public void RemoveMinionFromList(GameObject minion)
    {
        _listManager.RemoveMinionFromList(minion);
    }

    public void RemovePlayerFromList(GameObject player)
    {
        _listManager.RemovePlayerFromList(player);
    }

    public int GetMinionsCount()
    {
        return _listManager.GetMinionsCount();
    }

    public void AddPlayer(GameObject createdPlayer)
    {
        if (!IsServer) return;
        if(createdPlayer != null)
        {
            int randomID = Random.Range(100, 999);
            createdPlayer.GetComponent<PlayerCore>().SetID(randomID);
            _listManager.AddPlayerToList(createdPlayer);
            Debug.Log("Successfully created a player");
        }
    }

    public void AddMinionToList(GameObject minion)
    {
        _listManager.AddMinionToList(minion);
    }

    public GameObject GetProjectile1()
    {
        return projectile_1;
    }

    private GameObject InstatitatePlayer(GameObject playerPrefab, Vector3 position)
    {
        GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
        int id = Random.Range(100, 999);
        player.GetComponent<PlayerCore>().SetID(id);
        Debug.Log("Successfully instatitated player with the ID:" + id);
        return player;
    }

    public GameObject GetNextPlayerToPosition(Vector3 minionPosition)
    {
        return _listManager.GetNextPlayerToPosition(minionPosition);
    }

    //Returns the closest normal minion relative to own position
    public GameObject GetNextMinion(Vector3 pos)
    {
        return _listManager.GetNextMinionToPosition(pos);
    }

    public GameObject GetNextPlayer(Vector3 pos, int controllingPlayerID)
    {
        IEnumerable<GameObject> filteredPlayers = _listManager.GetPlayersList().Where(player => player.GetComponent<PlayerCore>().GetID() != controllingPlayerID);
        if(filteredPlayers.Count() >= 1)
        {
            return filteredPlayers.OrderBy(player => Vector3.Distance(pos, player.transform.position)).First();
        }
        return null;
    }

    //Returns the next Target player Minion
    public GameObject GetNextPlayerMinion(Vector3 pos, GameObject playerToAttack)
    {
        List<GameObject> playerMinions = playerToAttack.GetComponent<PlayerCore>().GetMinions();

        if(playerMinions.Count > 0)
        {
            return playerMinions.OrderBy(minion => Vector3.Distance(pos, minion.transform.position)).First();
        }
        return null;
    }

    private void RespawnPlayer(GameObject deadPlayer)
    {
        if (!IsServer || deadPlayer == null) return;

        float randomX = Random.Range(50f, 100f);
        float randomZ = Random.Range(50f, 100f);
        
        deadPlayer.SetActive(true);
        deadPlayer.transform.position = new Vector3(randomX, 3.5f, randomZ);
        deadPlayer.GetComponent<LifeComponent>().SetHealth(100f);
        deadPlayer.GetComponent<PlayerCore>().SetAlive(true);
    }

    public void DestroyAllPlayerMinions(GameObject player)
    {
        if (_listManager != null)
        {
            _listManager.DestroyAllPlayerMinions(player);
        }
    }

    public Color GetRandomColor()
    {

        if(_playerColors.Count <= 0)
        {
            InitColors();
        }

        int random = Random.Range(0, _playerColors.Count);
        Color color = _playerColors[random];
        _playerColors.RemoveAt(random);
        return color;
    }

    private void InitColors()
    {
        Color color1 = new Color(0f, 0f, 1f); // Blue
        Color color2 = new Color(1f, 1f, 0f); // Yellow
        Color color3 = new Color(0.5f, 0f, 0.5f); // Purple
        Color color4 = new Color(0f, 1f, 1f); // Cyan
        Color color5 = new Color(1f, 0.5f, 0f); // Orange
        Color color6 = new Color(0f, 0.5f, 0f); // Dark Green
        _playerColors.Add(color1);
        _playerColors.Add(color2);
        _playerColors.Add(color3);
        _playerColors.Add(color4);
        _playerColors.Add(color5);
        _playerColors.Add(color6);
    }
}