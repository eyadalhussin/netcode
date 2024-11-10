using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
{
    public GameObject _projectilePrefab;
    public Transform _shootingPos;

    private float _lastAttack = 0f;
    public float _attackInterval = 0.2f;
    void Update()
    {
        if (!IsOwner) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // Calculate the direction from the player to the mouse position
            Vector3 targetPosition = hit.point;
            Vector3 playerPositionOnPlane = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 shootDirection = (targetPosition - playerPositionOnPlane).normalized;

            // Set the y-component of the direction to zero
            shootDirection.y = 0f;

            // Calculate the rotation that faces the shooting direction
            Quaternion targetRotation = Quaternion.LookRotation(shootDirection, Vector3.up);

            // Limit the rotation to the y-axis
            targetRotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);

            transform.rotation = targetRotation;

            if (Input.GetButton("Fire1"))
            {
                CmdShootServerRpc();
            }

        }
    }

    [ServerRpc]
    void CmdShootServerRpc()
    {
        // Ensure the attack interval is respected
        if (Time.time - _lastAttack >= _attackInterval)
        {
            if (_projectilePrefab != null)
            {
                // Instantiate the projectile on the server
                GameObject bullet = Instantiate(_projectilePrefab, _shootingPos.position, Quaternion.identity);

                // Get the NetworkObject component and spawn the bullet across the network
                NetworkObject bulletNetworkObject = bullet.GetComponent<NetworkObject>();

                // Make sure the bullet has a NetworkObject attached before spawning
                if (bulletNetworkObject != null)
                {
                    bulletNetworkObject.Spawn(); // Spawn the bullet on the server and sync it to clients

                    // Set additional bullet properties
                    ProjectileCore projectileCoreComponent = bullet.GetComponent<ProjectileCore>();
                    projectileCoreComponent.SetDamageDealer(gameObject); // Pass the player as the damage dealer
                    projectileCoreComponent.SetForward(transform); // Set the forward direction for the projectile

                    _lastAttack = Time.time; // Update the time of the last attack
                }
                else
                {
                    DebugManager.LogError("The projectile prefab is missing a NetworkObject component.");
                }
            }
        }
    }
}