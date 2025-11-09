using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private Transform playerBody;

    [Header("FOV")]
    [SerializeField] private Camera cam;          // Auto-filled if left empty
    [SerializeField] private float baseFov = 90f; // idle FOV
    [SerializeField] private float sprintFov = 120f; // sprint FOV
    [SerializeField] private float fovLerpSpeed = 10f; // higher = snappier

    [SerializeField] private float freeRotateSensitivity = 0.5f; // mouse sensitivity in free mode
    [SerializeField] private float rollSpeedDegPerSec = 180f;  // Q/E roll speed
    private bool freeRotateActive = false;
    private Quaternion freeRot; // working rotation while in free mode

    private float verticalRotation = 0f;

    private void Awake()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam != null) cam.fieldOfView = baseFov;
    }

    private void LateUpdate()
    {
        // Input
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        // Are we allowed to free-rotate right now?
        bool wantsFree =    Input.GetMouseButton(1)           // RMB held
                         && !FirstPersonController.IsGroundedNow; // airborne

        if (wantsFree)
        {
            // --- ENTER free mode (one time) ---
            if (!freeRotateActive)
            {
                freeRotateActive = true;
                freeRot = transform.rotation; // start from current world rotation
            }

            // Use unscaled delta so feel is consistent in slow-mo
            float dt = Time.unscaledDeltaTime;

            // Update rotation in camera's local axes (trackball-like)
            // Rotate about local up (yaw) and local right (pitch), then local forward (roll)
            Vector3 localRight = freeRot * Vector3.right;
            Vector3 localUp = freeRot * Vector3.up;
            Vector3 localFwd = freeRot * Vector3.forward;

            // Mouse - yaw/pitch (remove pitch clamp so you can flip)
            freeRot = Quaternion.AngleAxis(mouseX * freeRotateSensitivity, localUp) *
                      Quaternion.AngleAxis(-mouseY * freeRotateSensitivity, localRight) *
                      freeRot;

            // Q/E - roll while holding RMB
            float rollInput = 0f;
            if (Input.GetKey(KeyCode.Q)) rollInput -= 1f;
            if (Input.GetKey(KeyCode.E)) rollInput += 1f;
            if (Mathf.Abs(rollInput) > 0f)
                freeRot = Quaternion.AngleAxis(rollInput * rollSpeedDegPerSec * dt, localFwd) * freeRot;

            // Apply
            transform.rotation = freeRot;

            // Keep FOV behavior you already have (slow-mo overrides sprint/base)
            float target = SloMo.IsActive ? SloMo.ActiveSlowMoFov
                                          : (FirstPersonController.IsSprinting ? sprintFov : baseFov);
            float current = cam.fieldOfView;
            current = Mathf.Lerp(current, target, 1f - Mathf.Exp(-fovLerpSpeed * Time.deltaTime));
            cam.fieldOfView = current;

            return; // IMPORTANT: skip normal look when in free mode
        }

        // --- EXIT free mode: snap camera back to "normal" orientation ---
        if (freeRotateActive && (!wantsFree || FirstPersonController.IsGroundedNow))
        {
            freeRotateActive = false;

            // Align the player body to the camera's current yaw
            float yaw = transform.eulerAngles.y;
            playerBody.rotation = Quaternion.Euler(0f, yaw, 0f);

            // Compute a sensible pitch (no roll), clamp to your usual range
            float pitch = -Mathf.Asin(transform.forward.y) * Mathf.Rad2Deg;
            verticalRotation = Mathf.Clamp(pitch, -85f, 85f);
            transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            // fall through to normal look below this frame
        }

        // --- NORMAL mouse look (your existing code path) ---
        playerBody.Rotate(Vector3.up * mouseX);
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -85f, 85f);
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // --- keep your FOV logic ---
        float targetFov = SloMo.IsActive ? SloMo.ActiveSlowMoFov
                                         : (FirstPersonController.IsSprinting ? sprintFov : baseFov);
        float f = cam.fieldOfView;
        f = Mathf.Lerp(f, targetFov, 1f - Mathf.Exp(-fovLerpSpeed * Time.deltaTime));
        cam.fieldOfView = f;
    }


}
