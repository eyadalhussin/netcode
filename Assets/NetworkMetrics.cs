using Unity.Netcode;
using UnityEngine;

public class NetworkMetrics : MonoBehaviour
{
    public float GetPing()
    {
        return NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.LocalClientId);
    }
}
