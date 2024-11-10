using Netcode.Transports.WebSocket;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using static HTTPHandler;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager instance;
    private LobbyUI _lobbyUI;
    private string masterServerUrl;
    private string gamesServerUrl;
    public List<Server> availableServers = new List<Server>();

    public NetworkList<LobbyPlayerData> LobbyPlayerList;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        LobbyPlayerList = new NetworkList<LobbyPlayerData>();
    }

    private void Start()
    {
        _lobbyUI = GetComponent<LobbyUI>();
        gamesServerUrl = EnvironmentSetting.gameServerUrl;
    }

    public void CreateServer()
    {
        if (instance == null) DebugManager.Log("LobbyManager is null");
        if (HTTPHandler.instance == null) DebugManager.Log("HTTPHandler is null");
        StartCoroutine(HTTPHandler.instance.HTTPCreateServer(
            onSuccess: (string msg) =>
            {
                DebugManager.Log(msg);
                FetchServers();
            },
            onFailure: (string err) =>
            {
                DebugManager.Log(err);
            }
        ));
    }

    public void FetchServers()
    {
        StartCoroutine(HTTPHandler.instance.HTTPGetAvailableServersList(
            onSuccess: (List<Server> servers) =>
            {
                availableServers = servers;
                _lobbyUI.SetUIStatus("PreConnection");
            },
            onFailure: (string err) =>
            {
                availableServers.Clear();
                _lobbyUI.SetUIStatus("PreConnection");
            }
        ));
    }

    internal void ConnectToGameServer(int port)
    {
        if (EnvironmentSetting.prod && NetworkManager.Singleton != null)
        {
            string networkaddress = $"https://{port}.{gamesServerUrl}";
            NetworkManager.Singleton.GetComponent<WebSocketTransport>().ConnectAddress = networkaddress;
            NetworkManager.Singleton.GetComponent<WebSocketTransport>().Port = 443;
            NetworkManager.Singleton.GetComponent<WebSocketTransport>().SecureConnection = true;
            DebugManager.Log($"Connecting as Client to production server {networkaddress} , WSS Enabled");
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.StartClient();
        }
        else if (!EnvironmentSetting.prod && NetworkManager.Singleton != null)
        {
            string networkaddress = "127.0.0.1";
            NetworkManager.Singleton.GetComponent<WebSocketTransport>().ConnectAddress = networkaddress;
            NetworkManager.Singleton.GetComponent<WebSocketTransport>().Port = (ushort)port;
            DebugManager.Log($"Connecting as Client to development server {networkaddress}:{port} , WSS Disabled");
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.StartClient();
        } else
        {
            DebugManager.Log("Cannot connect to the server, NetworkManager might be null");
        }
    }

    internal void ConnectToGameServerWithIP(string testIp, string port)
    {
        int parsedPort;
        int.TryParse(port, out parsedPort);

        DebugManager.Log("Connecting as Client to " + testIp + ":" + parsedPort);
        //NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(testIp, (ushort)parsedPort);
        NetworkManager.Singleton.GetComponent<WebSocketTransport>().ConnectAddress = testIp;
        NetworkManager.Singleton.GetComponent<WebSocketTransport>().Port = (ushort)parsedPort;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.StartClient();
    }

    public bool AllPlayersReady()
    {
        foreach (var player in LobbyPlayerList)
        {
            if (!player.IsReady)
            {
                return false;
            }
        }
        return true;
    }

    public void StartGame()
    {
        if (IsServer && AllPlayersReady())
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    /* Synchronization */
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    /* //Synchronization */

    /* NetworkConnection */

    // Starts the client and connects to a server
    public void StartClient(string ipAddress)
    {
        if (!string.IsNullOrEmpty(ipAddress))
        {
            // Set IP address and port for the client connection
            ushort port = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port;
            DebugManager.Log("Connecting as Client to " + ipAddress + ":" + port);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipAddress, port);
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        NetworkManager.Singleton.StartClient();
    }

    // Starts the host (server + client)
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    // Stops the client or host
    public void StopClient()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    /* // NetworkConnection */

    public void SetReadyStatusForClientId(ulong clientId)
    {
        SetReadyStatusForClientIdServerRpc(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyStatusForClientIdServerRpc(ulong clientId)
    {
        for (int i = 0; i < LobbyPlayerList.Count; i++)
        {
            if ((LobbyPlayerList[i].ClientId == clientId))
            {
                LobbyPlayerData newData = LobbyPlayerList[i];
                newData.IsReady = !newData.IsReady;
                LobbyPlayerList[i] = newData;
                StartGameIfAllPlayersAreReady();
                break;
            }
        }
    }
    public LobbyPlayerData GetPlayerObjectForClientId(ulong clientId)
    {
        foreach (var player in LobbyPlayerList)
        {
            if (player.ClientId == clientId) return player;
        }
        return new LobbyPlayerData(9999, "NOT FOUND", false);
    }

    public void StartGameIfAllPlayersAreReady()
    {
        if (IsServer)
        {
            bool ready = true;
            foreach (var player in LobbyPlayerList)
            {
                if (!player.IsReady)
                {
                    ready = false;
                }
            }
            if (ready) NetworkManager.Singleton.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log("Client Connected: " + clientId);
            LobbyPlayerData lobbyPlayer = new LobbyPlayerData(clientId, "Player " + clientId, false);
            LobbyPlayerList.Add(lobbyPlayer);
        }
        _lobbyUI.SetUIStatus("Connected");
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log("Client Disconnected: " + clientId);
            for (int i = (LobbyPlayerList.Count) - (1); i >= 0; i--)
            {
                if (LobbyPlayerList[i].ClientId == clientId)
                {
                    LobbyPlayerList.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
