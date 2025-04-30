using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;

    [Header("Position Settings")]
    [Tooltip("Camera position relative to the dolphin (X=right, Y=up, Z=back)")]
    public Vector3 cameraOffset = new Vector3(0f, 3.5f, -7.0f);
    [Tooltip("How high above the dolphin to look at")]
    public float lookUpOffset = 1.0f;
    [Tooltip("How far in front of the dolphin to look at")]
    public float lookForwardOffset = 2.0f;

    [Header("Smoothing")]
    [Range(0.1f, 20f)]
    public float positionSmoothSpeed = 10f;
    [Range(0.1f, 20f)]
    public float rotationSmoothSpeed = 8f;

    [Header("Collision")]
    public LayerMask collisionLayers = ~0;
    public float collisionOffset = 0.2f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (target == null)
        {
            Debug.LogError("No target assigned to the camera!");
            enabled = false;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired position (offset in the dolphin's local space)
        Vector3 desiredPosition = target.position + target.rotation * cameraOffset;

        // Handle collision
        Vector3 adjustedPosition = HandleCollision(target.position, desiredPosition);

        // Smooth camera position
        transform.position = Vector3.Lerp(
            transform.position,
            adjustedPosition,
            positionSmoothSpeed * Time.deltaTime
        );

        // Calculate look target (slightly ahead and above the dolphin)
        Vector3 lookTarget = target.position +
                            (target.up * lookUpOffset) +
                            (target.forward * lookForwardOffset);

        // Only rotate if we're not at the exact look position (avoid LookRotation errors)
        if ((lookTarget - transform.position).sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSmoothSpeed * Time.deltaTime
            );
        }
    }

    Vector3 HandleCollision(Vector3 targetPos, Vector3 desiredPos)
    {
        // Check for obstacles between target and desired camera position
        Vector3 direction = desiredPos - targetPos;
        float distance = direction.magnitude;

        RaycastHit hit;
        if (Physics.Raycast(targetPos, direction.normalized, out hit, distance, collisionLayers))
        {
            // Move camera to hit point, but slightly back to avoid clipping
            return hit.point - (direction.normalized * collisionOffset);
        }

        // No collision, return the original desired position
        return desiredPos;
    }
}