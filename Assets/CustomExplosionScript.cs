using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CustomExplosionScript : NetworkBehaviour
{
    private void Start()
    {
        StartCoroutine(DestroyThroughNetwork());   
    }

    private IEnumerator DestroyThroughNetwork()
    {
        yield return new WaitForSeconds(2f);

        NetworkObject obj = gameObject.GetComponent<NetworkObject>();
        if(obj != null && IsServer)
        {
            obj.Despawn();
        }
    }
}
