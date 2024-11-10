using Unity.Netcode;
using UnityEngine;

public class LobbyUI : NetworkBehaviour
{
    private string ipAddress = "127.0.0.1"; // Default IP for client to connect to
    private string status = "PreConnection";

    private string testIp = "127.0.0.1";  // Default value for IP
    private string testPort = "9000";      // Default value for Port

    private void OnGUI()
    {
        GUIStyle bigLabel = new GUIStyle(GUI.skin.label);
        bigLabel.fontSize = 18;
        bigLabel.normal.textColor = Color.green;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.margin.left = 10;

        // Define the main container area
        GUILayout.BeginArea(new Rect(50, 50, 500, 400), GUI.skin.box);
        GUILayout.Label("Netcode for GameObjects", bigLabel);

        // Check the connection status and draw the appropriate UI
        switch (status)
        {
            case "PreConnection":
                DrawPreConnectionUI();
                break;
            case "Connected":
                DrawPostConnectionUI();
                break;
            default:
                // Optionally, handle other statuses or default case
                break;
        }

        GUILayout.EndArea();
    }

    // Draw the UI for pre-connection (Connect Client, Start Host)
    private void DrawPreConnectionUI()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("IP Address:");
        testIp = GUILayout.TextField(testIp, GUILayout.Width(150));  // IP input field

        // Label and Input field for Port
        GUILayout.Label("Port:");
        testPort = GUILayout.TextField(testPort, GUILayout.Width(70));  // Port input field

        if (GUILayout.Button("Connect", GUILayout.Width(100)))
        {
            LobbyManager.instance.ConnectToGameServerWithIP(testIp, testPort);
        }
        GUILayout.EndHorizontal();

        // Server list and controls
        GUILayout.BeginVertical(); // Use vertical layout to stack elements


        // Server list
        if (LobbyManager.instance.availableServers.Count > 0)
        {
            int i = 1;
            foreach (var server in LobbyManager.instance.availableServers)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Server: {i++} - Port: {server.port} - Status: {server.status}", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Connect", GUILayout.Width(100)))
                {
                    LobbyManager.instance.ConnectToGameServer(server.port);
                    DebugManager.Log($"Connecting to server on port {server.port}");
                }
                GUILayout.EndHorizontal();
            }
        }
        else
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("No servers are currently Running");
            GUILayout.EndHorizontal();
        }

        // Server creation and reload controls at the top
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Server", GUILayout.Width(150)))
        {
            Debug.Log("Creating Server...");
            LobbyManager.instance.CreateServer();
        }

        if (GUILayout.Button("Reload", GUILayout.Width(150)))
        {
            LobbyManager.instance.FetchServers();
        }
        GUILayout.EndHorizontal();


        GUILayout.EndVertical();
    }


    // Draw the UI for post-connection (Stop Client, Ready Button, Player List)
    private void DrawPostConnectionUI()
    {
        if (GUILayout.Button("Stop Client"))
        {
            status = "PreConnection";
            OnStopClient();
        }

        bool ready = false;
        LobbyManager.instance.GetPlayerObjectForClientId(NetworkManager.Singleton.LocalClientId);


        if (GUILayout.Button(ready ? "Not Ready" : "Ready"))
        {
            OnReadyButtonClicked();
        }

        // Show the list of connected players
        GUILayout.Label("Connected Players:");
        DrawPlayersListGUI();
    }

    // Dynamically update the list of connected players
    public void DrawPlayersListGUI()
    {
        foreach (var player in LobbyManager.instance.LobbyPlayerList)
        {
            GUILayout.Label($"Name: {player.PlayerName} Id: {player.ClientId} Status: {(player.IsReady ? "Ready" : "Not Ready")}");
        }
    }

    // Logic for connecting as a client
    private void OnConnectClient()
    {
        LobbyManager.instance.StartClient(ipAddress);
    }

    // Logic for starting a host
    private void OnStartHost()
    {
        LobbyManager.instance.StartHost();
    }

    // Logic for stopping the client or host
    private void OnStopClient()
    {
        LobbyManager.instance.StopClient();
    }

    // Toggle ready status
    private void OnReadyButtonClicked()
    {
        LobbyManager.instance.SetReadyStatusForClientIdServerRpc(NetworkManager.Singleton.LocalClientId);
    }
    public void SetUIStatus(string _status)
    {
        this.status = _status;
    }
}
