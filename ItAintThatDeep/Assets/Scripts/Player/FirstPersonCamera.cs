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

    private float verticalRotation = 0f;

    private void Awake()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam != null) cam.fieldOfView = baseFov;
    }

    private void LateUpdate()
    {
        // --- Mouse look ---
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        playerBody.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -85f, 85f);
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // --- FOV: clean 2-state lerp (no pulse) ---
        bool sprinting = FirstPersonController.IsSprinting;
        float target = sprinting ? sprintFov : baseFov;

        // Exponential ease toward target (frame-rate independent)
        float current = cam.fieldOfView;
        current = Mathf.Lerp(current, target, 1f - Mathf.Exp(-fovLerpSpeed * Time.deltaTime));
        cam.fieldOfView = current;
    }
}
