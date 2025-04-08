using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    // --- NEW: Reference to player shooting script ---
    [Tooltip("Reference to the DolphinShooting script on the player.")]
    public DolphinShooting playerShooting; // Assign this in the Inspector

    [Header("Normal Positioning")]
    public Vector3 normalOffset = new Vector3(0f, 3.0f, -8.0f);
    public float normalFOV = 60f; // Default Field of View
    public float normalPositionDamping = 10.0f;
    public float normalRotationDamping = 7.0f;


    [Header("Aiming Positioning")]
    [Tooltip("Camera offset relative to target when aiming.")]
    public Vector3 aimingOffset = new Vector3(0.5f, 2.5f, -4f); // Closer and maybe slightly to side
    [Tooltip("Camera Field of View when aiming.")]
    public float aimingFOV = 40f; // Zoomed-in FOV
    [Tooltip("Optional: Different damping when aiming (higher = snappier). Set <= 0 to use normal damping.")]
    public float aimingPositionDamping = 15.0f;
    [Tooltip("Optional: Different damping when aiming (higher = snappier). Set <= 0 to use normal damping.")]
    public float aimingRotationDamping = 10.0f;


    [Header("Smoothing")]
    [Tooltip("How quickly the FOV changes between normal and aiming.")]
    public float fovSmoothSpeed = 10f;
    [Tooltip("How quickly the camera offset changes between normal and aiming.")]
    public float offsetSmoothSpeed = 8f;


    [Header("Look At")]
    public float lookAtHeightOffset = 1.5f;
    public float lookAtForwardOffset = 3.0f;

    [Header("Collision")]
    public LayerMask collisionLayers = ~0;
    public float collisionBuffer = 0.3f;
    public float collisionMoveSpeed = 20f;

    // --- Private Variables ---
    private Vector3 currentVelocity = Vector3.zero;
    private Camera cam; // Reference to the Camera component
    private Vector3 currentOffset; // The offset we smoothly transition

    // --- Cinematic Pan Variables ---
    private bool isCinematic = false;
    // ... (Keep cinematic variables as they were) ...
    private Vector3 originalOffsetCinematic;
    private Transform cinematicTarget;
    private float cinematicTimer = 0f;
    private float cinematicDuration = 1f;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("ThirdPersonCamera requires a Camera component!", this);
            this.enabled = false; // Disable script if no camera
            return;
        }
        // Initialize current offset to the normal offset
        currentOffset = normalOffset;
    }

    void LateUpdate()
    {
        if (target == null) { /* ... No target warning ... */ return; }

        // --- Check Player Shooting Reference ---
        if (playerShooting == null)
        {
            Debug.LogError("ThirdPersonCamera needs the 'Player Shooting' reference assigned!", this);
            // Optionally try to find it automatically, but assignment is better
            // playerShooting = target.GetComponentInChildren<DolphinShooting>();
            // if (playerShooting == null) return; // Still couldn't find it
            return;
        }


        if (isCinematic) { HandleCinematicPan(); return; }

        FollowTarget();
    }

    void FollowTarget()
    {
        // --- Determine Target State based on Aiming ---
        bool isAiming = playerShooting.IsAiming; // Get aiming state

        float targetFOV = isAiming ? aimingFOV : normalFOV;
        Vector3 targetOffset = isAiming ? aimingOffset : normalOffset;
        float positionDamp = (isAiming && aimingPositionDamping > 0) ? aimingPositionDamping : normalPositionDamping;
        float rotationDamp = (isAiming && aimingRotationDamping > 0) ? aimingRotationDamping : normalRotationDamping;

        // --- Smoothly Transition FOV ---
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovSmoothSpeed * Time.deltaTime);

        // --- Smoothly Transition Offset ---
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, offsetSmoothSpeed * Time.deltaTime);


        // --- Calculate Desired Camera Position (using smoothed offset) ---
        Quaternion targetRotation = target.rotation;
        Vector3 desiredPosition = target.position + (targetRotation * currentOffset); // Use currentOffset

        // --- Handle Camera Collision ---
        Vector3 adjustedDesiredPosition = HandleCollision(target.position + (target.up * lookAtHeightOffset), desiredPosition);

        // --- Smoothly Move Camera Position (using determined damping) ---
        transform.position = Vector3.SmoothDamp(transform.position, adjustedDesiredPosition, ref currentVelocity, 1.0f / positionDamp);

        // --- Calculate Desired Camera Rotation (using determined damping) ---
        Vector3 lookAtPoint = target.position + (targetRotation * Vector3.forward * lookAtForwardOffset) + (target.up * lookAtHeightOffset);
        if ((lookAtPoint - transform.position).sqrMagnitude > 0.01f)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(lookAtPoint - transform.position, target.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationDamp * Time.deltaTime);
        }
    }

    // ... (Keep HandleCollision, PlayCinematicPan, HandleCinematicPan as they were) ...
    Vector3 HandleCollision(Vector3 raycastOrigin, Vector3 desiredPosition) { /* ... */ return desiredPosition; } // Placeholder
    public void PlayCinematicPan(Transform newTarget, float duration) { /* ... */ }
    void HandleCinematicPan() { /* ... */ }
}