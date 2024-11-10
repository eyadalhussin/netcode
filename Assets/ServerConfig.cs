using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Netcode.Transports.WebSocket;

public class ServerConfig : MonoBehaviour
{
    bool isServer = false;
    int port = 9000;

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        string[] args = Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {  
            // Handling -port Argument
            if (args[i].ToLower() == "-port" && i + 1 < args.Length) {
                if (int.TryParse(args[i+1], out int parsedPort)){
                    port = parsedPort;
                    Debug.Log($"Setting port to {port}");
                } else
                {
                    Debug.LogError("Invalid port argument");
                }
            }

            // Handling -server Argument
            if (args[i].ToLower() == "-server")
            {
                isServer = true;
            }   
        }

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "Room")
        {
            NetworkManager.Singleton.GetComponent<WebSocketTransport>().Port = (ushort)port;
            if (isServer && !NetworkManager.Singleton.IsServer)
            {
                Debug.Log("Starting as Server");
                NetworkManager.Singleton.StartServer();
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
