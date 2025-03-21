using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target; // Reference to the player
    public Vector3 offset = new Vector3(3, 6, -15); // Shifted slightly right, higher, and further back
    public float smoothSpeed = 5f; // How smoothly the camera follows
    public float rotationSpeed = 5f; // How smoothly the camera rotates
    public LayerMask collisionLayers; // Layer mask for objects the camera should collide with
    public float collisionBuffer = 0.2f; // Small buffer to avoid clipping

    void LateUpdate()
    {
        if (target == null) return; // Safety check

        // Rotate the offset based on the player's rotation
        Vector3 rotatedOffset = target.rotation * offset;

        // Desired position behind the player
        Vector3 desiredPosition = target.position + rotatedOffset;

        // Check for collisions
        RaycastHit hit;
        if (Physics.Linecast(target.position, desiredPosition, out hit, collisionLayers))
        {
            // If a collision is detected, place the camera at the hit point minus a small buffer
            Vector3 hitPoint = hit.point;
            Vector3 direction = (desiredPosition - target.position).normalized;
            transform.position = hitPoint - direction * collisionBuffer;
        }
        else
        {
            // Smoothly move the camera to the desired position if no collision is detected
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }

        // Make the camera look slightly ahead of the dolphin instead of directly at it
        Vector3 lookTarget = target.position + target.forward * 5f + Vector3.up * 2f;
        transform.LookAt(lookTarget);
    }
}
