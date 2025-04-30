using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DolphinMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base movement speed")]
    public float moveSpeed = 10f;
    [Tooltip("Up/down movement speed")]
    public float verticalSpeed = 8f;
    [Tooltip("Mouse sensitivity for rotation")]
    public float lookSensitivity = 2.0f;
    [Tooltip("How smoothly the dolphin rotates (lower = smoother)")]
    [Range(0.01f, 1.0f)]
    public float rotationSmoothing = 0.15f;

    [Header("Animation")]
    public string swimAnimationName = "Armature_Dolphin|Armature_Dolphin|Armature_Dolphin|Idle";
    public float idleAnimSpeed = 1.0f;
    public float normalSwimSpeed = 1.5f;

    [Header("Audio")]
    public AudioSource swimSound;
    public AudioSource verticalSound;

    // References
    private Rigidbody rb;
    private Animation anim;
    private Sprinting sprintSystem;

    // Internal state
    private float rotationX = 0f; // Yaw (horizontal rotation)
    private float rotationY = 0f; // Pitch (up/down rotation)
    private Vector3 moveInput;
    private float verticalInput;
    private Quaternion targetRotation;
    private bool movementFrozen = false;

    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animation>();
        sprintSystem = GetComponent<Sprinting>();

        // Configure rigidbody for underwater movement
        rb.useGravity = false;
        rb.drag = 2.0f;
        rb.angularDrag = 4.0f;

        // Lock cursor for mouse look
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize rotation values
        targetRotation = transform.rotation;
        rotationX = transform.eulerAngles.y;
        rotationY = transform.eulerAngles.x;

        // Clamp initial pitch to valid range
        if (rotationY > 180) rotationY -= 360;
        rotationY = Mathf.Clamp(rotationY, -89f, 89f);

        // Set up animation
        if (anim != null && !string.IsNullOrEmpty(swimAnimationName))
        {
            if (anim[swimAnimationName] != null)
            {
                anim[swimAnimationName].wrapMode = WrapMode.Loop;
                anim.Play(swimAnimationName);
            }
            else
            {
                Debug.LogWarning($"Animation '{swimAnimationName}' not found!");
            }
        }
    }

    void Update()
    {
        // Skip input processing if frozen
        if (movementFrozen) return;

        // Make sure cursor is locked and hidden during gameplay
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Mouse look input
        rotationX += Input.GetAxis("Mouse X") * lookSensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * lookSensitivity;
        rotationY = Mathf.Clamp(rotationY, -89f, 89f);
        targetRotation = Quaternion.Euler(rotationY, rotationX, 0);

        // Movement input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float forward = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(horizontal, 0, forward).normalized;

        // Vertical movement (Space=up, Ctrl/C/F=down)
        verticalInput = 0f;
        if (Input.GetKey(KeyCode.Space)) verticalInput = 1f;
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.F)) verticalInput = -1f;

        // Update animation speed based on movement and sprint state
        UpdateAnimation();

        // Update audio
        UpdateAudio();
    }

    void FixedUpdate()
    {
        if (movementFrozen)
        {
            // If frozen, just decelerate to stop
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 10f);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 10f);
            return;
        }

        // Smooth rotation
        float rotationFactor = 1.0f - Mathf.Pow(rotationSmoothing, Time.fixedDeltaTime);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationFactor));

        // Calculate movement force
        Vector3 moveForce = (transform.forward * moveInput.z + transform.right * moveInput.x) * moveSpeed;
        Vector3 verticalForce = Vector3.up * verticalInput * verticalSpeed;

        // Apply sprint multiplier if sprinting
        float sprintMultiplier = 1.0f;
        if (sprintSystem != null && sprintSystem.IsSprinting())
        {
            sprintMultiplier = sprintSystem.GetSprintMultiplier();
        }

        // Apply forces
        rb.AddForce((moveForce + verticalForce) * sprintMultiplier, ForceMode.Acceleration);
    }

    void UpdateAnimation()
    {
        if (anim == null || string.IsNullOrEmpty(swimAnimationName) || anim[swimAnimationName] == null) return;

        bool isMoving = moveInput.magnitude > 0.1f || Mathf.Abs(verticalInput) > 0.1f;
        bool isSprinting = sprintSystem != null && sprintSystem.IsSprinting();

        // Set animation speed based on movement state
        if (!isMoving)
        {
            anim[swimAnimationName].speed = idleAnimSpeed;
        }
        else if (isSprinting)
        {
            // Use the sprint animation speed from the sprint system
            anim[swimAnimationName].speed = normalSwimSpeed * sprintSystem.GetSprintMultiplier();
        }
        else
        {
            anim[swimAnimationName].speed = normalSwimSpeed;
        }
    }

    void UpdateAudio()
    {
        bool isMovingHorizontally = moveInput.magnitude > 0.1f;
        bool isMovingVertically = Mathf.Abs(verticalInput) > 0.1f;

        // Play/stop horizontal movement sound
        if (swimSound != null)
        {
            if (isMovingHorizontally && !swimSound.isPlaying)
                swimSound.Play();
            else if (!isMovingHorizontally && swimSound.isPlaying)
                swimSound.Stop();
        }

        // Play/stop vertical movement sound
        if (verticalSound != null)
        {
            if (isMovingVertically && !verticalSound.isPlaying)
                verticalSound.Play();
            else if (!isMovingVertically && verticalSound.isPlaying)
                verticalSound.Stop();
        }
    }

    // Public method to freeze/unfreeze movement
    public void SetMovementFreeze(bool freeze)
    {
        movementFrozen = freeze;

        if (freeze)
        {
            // Stop all audio
            if (swimSound != null) swimSound.Stop();
            if (verticalSound != null) verticalSound.Stop();

            // Stop physics
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            // Resume physics
            rb.isKinematic = false;
        }
    }

    // Public method to check if movement is enabled
    public bool IsMovementEnabled()
    {
        return !movementFrozen;
    }
}