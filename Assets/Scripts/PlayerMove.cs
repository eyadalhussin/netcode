using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 15.0f; // Adjust the movement speed as needed.

    void Update()
    {
        // Get input from the player.
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate the movement direction based on input.
        Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;

        // Move the kinematic object based on the calculated direction.
        MoveObject(moveDirection);
    }

    void MoveObject(Vector3 direction)
    {
        // Calculate the new position based on the direction and movement speed.
        Vector3 newPosition = transform.position + direction * moveSpeed * Time.deltaTime;

        // Update the object's position using the Transform component.
        transform.position = newPosition;
    }
}
