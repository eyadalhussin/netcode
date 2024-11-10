using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkMetricsUI : MonoBehaviour
{
    NetworkMetrics _networkMetrics;
    private void Start()
    {
        _networkMetrics = GetComponent<NetworkMetrics>();
    }
    private void OnGUI()
    {
        if (_networkMetrics == null) return;
        // Display the ping value
        float ping = _networkMetrics.GetPing();
        GUI.Label(new Rect(10, 10, 200, 20), "Ping: " + ping.ToString("F2") + " ms");
        if(GUI.Button(new Rect(10,30,200,30), "Stop Client"))
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("Room");
        }
    }
}
