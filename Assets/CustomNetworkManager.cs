using Netcode.Transports.WebSocket;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CustomNetworkManager : NetworkManager
{
    private void Awake()
    {
        LogLevel = LogLevel.Developer;
    }

    private void Start()
    {
        Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (IsServer)
        {
            // Log the disconnection event
            Debug.Log($"Client {clientId} disconnected.");

            // Check if this is the last client
            if (NetworkManager.Singleton.ConnectedClients.Count <= 1)
            {
                // Start coroutine to handle server shutdown
                StartCoroutine(ShutdownServer());
            }
        }
    }

    private IEnumerator ShutdownServer()
    {
        ushort port = 0;
        yield return null;
        // Log and shutdown server
        Debug.Log("All Clients disconnected, shutting down the server.");

        // Shutdown the transport as well
        var transport = Singleton.GetComponent<WebSocketTransport>();
        if (transport != null)
        {
            port = transport.Port;
            transport.Shutdown();
        }

        HTTPHandler.instance.ShutdownServer(port);
        Singleton.Shutdown();

        // Add Here the request to shutdown the server
        Debug.Log("Server shutdown initiated.");
    }
}
