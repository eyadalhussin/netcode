using Cinemachine;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CameraComponent : NetworkBehaviour
{
    private CinemachineVirtualCamera vrCamera;
    private int maxRetries = 100;
    private float retryInterval = 0.3f;


    public void AssignVirtualCameraToPlayer()
    {
        StartCoroutine(FindAndAssignCamera());
    }

    private IEnumerator FindAndAssignCamera()
    {
        int attemps = 0;
        // This will keep executing in Interval of {retryInterval} as Long the Camera is not found
        while(vrCamera == null && attemps < maxRetries)
        {
            vrCamera = FindObjectOfType<CinemachineVirtualCamera>();
            if (vrCamera != null)
            {
                if (IsLocalPlayer)
                {
                    DebugManager.Log("Camera Found, Trying to assign to the player");
                    vrCamera.Follow = gameObject.transform;
                    vrCamera.LookAt = gameObject.transform;
                    DebugManager.Log("Finished Assigning Camera to the Player");
                    //CameraHealthbar cameraHealthbar = gameObject.transform.Find("HealthCanvas").GetComponent<CameraHealthbar>();
                    // Now find all health bars in the scene and assign the camera to them
                    CameraHealthbar[] allHealthbars = FindObjectsOfType<CameraHealthbar>();
                    foreach (var healthbar in allHealthbars)
                    {
                        healthbar.AssignCameraToHealthbar(vrCamera);
                    }
                    yield break;
                }
            }
            // If the Camera is not found, it will retry again after {retryInterval}
            attemps++;
            yield return new WaitForSeconds(retryInterval);
        }
        // This Code will be called if it exceeds the maxRetries
        if(vrCamera = null)
        {
            DebugManager.LogWarning("Failed to find the Cinemachine Virtual Camera");
        }
    }

    /*
    public void AssignVirtualCameraToPlayer()
    {
        if (IsLocalPlayer)
        {
            DebugManager.Log("IsLocalPlayer: Trying to find the Camera");
            vrCamera = FindObjectOfType<CinemachineVirtualCamera>();
            if (vrCamera != null)
            {
                DebugManager.Log("Camera Found, Trying to assign to the player");
                vrCamera.Follow = gameObject.transform;
                vrCamera.LookAt = gameObject.transform;
                DebugManager.Log("Finished Assigning Camera to the Player");
            }
        }
    }*/
}
