using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkManagerUI : MonoBehaviour
{
    private static NetworkManager _networkManager;

    private void Awake()
    {
        _networkManager = GetComponent<NetworkManager>();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if(!_networkManager.IsClient && !_networkManager.IsServer)
        {
            StartButtons();
        } else
        {
            StatusLabels();
        }
        GUILayout.EndArea();
    }
    
    static void StartButtons()
    {
        if (GUILayout.Button("Host")) _networkManager.StartHost();
        if (GUILayout.Button("Client")) _networkManager.StartClient();
        if (GUILayout.Button("Server")) _networkManager.StartServer();

    }

    static void StatusLabels()
    {
        var mode = _networkManager.IsHost ?
            "Host" : _networkManager.IsServer ? "Server" : "Client";
        GUILayout.Label("Transport: " + _networkManager.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

}
