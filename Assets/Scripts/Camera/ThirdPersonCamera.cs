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

    private bool isCinematic = false;
    private Vector3 originalOffset;
    private Transform cinematicTarget;
    private float cinematicTimer = 0f;
    private float cinematicDuration = 1f;


    public void PlayCinematicPan(Transform newTarget, float duration)
    {
        if (target == null || newTarget == null) return;

        isCinematic = true;
        originalOffset = offset;
        cinematicTarget = newTarget;
        cinematicTimer = duration;
        cinematicDuration = duration;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (isCinematic)
        {
            cinematicTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(1f - (cinematicTimer / cinematicDuration));
            t = Mathf.SmoothStep(0, 1, t);


            // Smooth interpolate between player and new target
            Vector3 fromPos = target.position + (target.rotation * offset);
            Vector3 toPos = cinematicTarget.position + Vector3.up * 4f + cinematicTarget.forward * -6f;
            transform.position = Vector3.Lerp(fromPos, toPos, Mathf.SmoothStep(0, 1, t));
            transform.LookAt(cinematicTarget.position + Vector3.up * 1.5f);

            if (cinematicTimer <= 0f)
            {
                isCinematic = false;
            }

            return;
        }

        // Follow logic
        Vector3 rotatedOffset = target.rotation * offset;
        Vector3 desiredPosition = target.position + rotatedOffset;

        RaycastHit hit;
        if (Physics.Linecast(target.position, desiredPosition, out hit, collisionLayers))
        {
            Vector3 hitPoint = hit.point;
            Vector3 direction = (desiredPosition - target.position).normalized;
            transform.position = hitPoint - direction * collisionBuffer;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }

        Vector3 lookTarget = target.position + target.forward * 5f + Vector3.up * 2f;
        transform.LookAt(lookTarget);
    }

}
