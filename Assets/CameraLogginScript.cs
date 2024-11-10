using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraLogginScript : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        DebugManager.Log("Camera Spawned");
    }
}
