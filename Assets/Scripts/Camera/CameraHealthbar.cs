using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraHealthbar : NetworkBehaviour
{
    private Transform _camera;
    /*
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GameObject virtualCamera = GameObject.FindWithTag("Virtual Camera");
        if (virtualCamera)
        {
            _camera = virtualCamera.transform;
        }
    }*/

    public void AssignCameraToHealthbar(CinemachineVirtualCamera vrCamera)
    {
        if (vrCamera)
        {
            _camera = vrCamera.transform;
        }
        else
        {
            DebugManager.Log("No Camera were found for the Healthbar !");
        }
    }

    private void LateUpdate()
    {
        if (_camera)
        {
            transform.LookAt(transform.position + _camera.forward);
            //transform.rotation = Quaternion.identity;
        }
    }
}
