using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Sprinting))]
[RequireComponent(typeof(DolphinShooting))] // Added dependency
public class DolphinMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    [Tooltip("Base speed factor used in force calculation for horizontal movement.")]
    public float moveSpeed = 10f;
    [Tooltip("Base speed factor used in force calculation for vertical movement.")]
    public float verticalSpeed = 8f;
    [Tooltip("Mouse sensitivity for looking around.")]
    public float lookSensitivity = 2.0f;
    [Tooltip("Controls how smoothly the dolphin visually rotates towards the look direction. Lower = slower/smoother.")]
    [Range(0.01f, 1.0f)]
    public float rotationSmoothFactor = 0.15f;

    [Header("Physics & Feel")]
    [Tooltip("Overall multiplier for movement forces. Adjust AFTER changing AddForce calculation! Needs much lower value now.")]
    public float moveForceMultiplier = 1.0f; // START LOW (e.g., 1-10) AND TUNE UP
    [Tooltip("Resistance to linear motion.")]
    public float drag = 2.0f;
    [Tooltip("Resistance to rotational motion.")]
    public float angularDrag = 4.0f;

    [Header("Aiming")] // New section for aiming speed
    [Tooltip("Multiplier applied to movement force when aiming. 1 = no change, < 1 = slower.")]
    [Range(0.1f, 1.0f)]
    public float aimingMoveForceMultiplier = 0.6f;

    [Header("Animation")]
    public string swimAnimationName = "Armature_Dolphin|Armature_Dolphin|Armature_Dolphin|Idle"; // MAKE SURE THIS NAME IS CORRECT
    public float idleAnimationSpeed = 1.0f;
    public float normalAnimationSpeed = 1.5f;
    public float sprintAnimationMultiplier = 3f; // Animation speed multiplier during sprint

    [Header("Audio")]
    public AudioSource movementSound; // Looping horizontal movement
    public AudioSource verticalMovementSound; // Looping vertical movement

    // --- Private Variables ---
    private Rigidbody rb;
    private Animation dolphinAnimation; // Legacy Animation component
    private Sprinting sprintingScript;
    private DolphinShooting shootingScript; // Reference to Shooting Script

    private float rotationX = 0f;
    private float rotationY = 0f;
    private Vector3 moveInput;
    private float verticalInput;
    private Quaternion targetRotation; // Desired rotation based on mouse input
    private bool isMovementFrozen = false; // Track freeze state internally

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sprintingScript = GetComponent<Sprinting>();
        shootingScript = GetComponent<DolphinShooting>(); // Get Shooting Script
        dolphinAnimation = GetComponentInChildren<Animation>();

        if (sprintingScript == null)
        {
            Debug.LogError("DolphinMovement requires Sprinting script on the same GameObject.", this);
        }
        if (shootingScript == null)
        { // Added check
            Debug.LogError("DolphinMovement requires DolphinShooting script on the same GameObject.", this);
        }
        ValidateAnimationComponent();

        // --- Configure Rigidbody ---
        rb.useGravity = false;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
        // **IMPORTANT:** Ensure Rigidbody Interpolate is set to 'Interpolate' or 'Extrapolate' in Inspector for smooth visuals!
        // **IMPORTANT:** Ensure Rigidbody Constraints Freeze Position/Rotation are UNCHECKED in Inspector.

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        targetRotation = transform.rotation; // Initialize target rotation
        rotationX = transform.eulerAngles.y; // Initialize rotationX based on starting orientation
        rotationY = transform.eulerAngles.x; // Initialize rotationY based on starting orientation
        // Clamp initial pitch if needed
        if (rotationY > 180) rotationY -= 360;
        rotationY = Mathf.Clamp(rotationY, -89f, 89f);
    }

    void Update() // Handle Input in Update
    {
        // If movement is frozen (e.g., by dialogue), skip input processing
        if (isMovementFrozen) return;

        // --- Rotation Input (Mouse Look) ---
        rotationX += Input.GetAxis("Mouse X") * lookSensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * lookSensitivity;
        rotationY = Mathf.Clamp(rotationY, -89f, 89f); // Prevent gimbal lock/flipping

        // Calculate desired rotation based on accumulated input
        targetRotation = Quaternion.Euler(rotationY, rotationX, 0);

        // --- Movement Input (Keyboard) ---
        float horizontal = Input.GetAxisRaw("Horizontal");
        float forward = Input.GetAxisRaw("Vertical");
        // Allow strafing by default, could be disabled if desired (set horizontal = 0)
        moveInput = new Vector3(horizontal, 0, forward).normalized;

        // Vertical Movement Input
        verticalInput = 0f;
        if (Input.GetKey(KeyCode.Space)) verticalInput = 1f;
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.F)) verticalInput = -1f;

        // --- Update Animation Speed ---
        UpdateAnimationSpeed();

        // --- Update Audio ---
        UpdateAudio();
    }

    void FixedUpdate() // Apply Physics in FixedUpdate
    {
        // If movement is frozen, do not apply physics updates
        if (isMovementFrozen) return;

        // --- Apply Rotation ---
        // Use Slerp with an exponential decay factor for smoother, frame-rate independent damping
        float t = 1.0f - Mathf.Pow(rotationSmoothFactor, Time.fixedDeltaTime);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, t));

        // --- Calculate Movement Direction ---
        Vector3 localMove = (transform.forward * moveInput.z + transform.right * moveInput.x) * moveSpeed;
        Vector3 verticalMove = Vector3.up * verticalInput * verticalSpeed; // World vertical

        // --- Determine Movement Multipliers ---
        float currentSprintMultiplier = sprintingScript != null ? sprintingScript.GetSprintMultiplier() : 1.0f;
        // ADD Aiming Multiplier Check
        float currentAimMultiplier = (shootingScript != null && shootingScript.IsAiming) ? aimingMoveForceMultiplier : 1.0f;

        // ** APPLY MULTIPLIERS to Force Calculation **
        // Calculate the final force vector *without* Time.fixedDeltaTime
        Vector3 forceVector = (localMove + verticalMove) * currentSprintMultiplier * currentAimMultiplier * moveForceMultiplier; // Added currentAimMultiplier

        // Apply the force using Acceleration mode (ignores mass, applies velocity change over time)
        rb.AddForce(forceVector, ForceMode.Acceleration);
    }

    void ValidateAnimationComponent()
    {
        if (dolphinAnimation == null)
        {
            Debug.LogWarning("Dolphin Animation component not found on this object or its children! Animation will not play.", this);
        }
        else if (string.IsNullOrEmpty(swimAnimationName) || dolphinAnimation[swimAnimationName] == null)
        { // Check for empty string too
            Debug.LogWarning($"Animation clip '{swimAnimationName}' not found or name is empty! Please check the name in the Inspector and Animation component.", this);
        }
        else
        {
            dolphinAnimation[swimAnimationName].wrapMode = WrapMode.Loop;
            Debug.Log($"Validated Animation Clip: {swimAnimationName}");
        }
    }

    void UpdateAnimationSpeed()
    {
        // Guard clause if animation component or clip is missing
        if (dolphinAnimation == null || string.IsNullOrEmpty(swimAnimationName) || dolphinAnimation[swimAnimationName] == null) return;

        // Determine movement state based on *input*, not velocity, for snappier animation response
        bool isCurrentlyMovingInput = moveInput.magnitude > 0.1f || Mathf.Abs(verticalInput) > 0.1f;
        bool isSprintingState = sprintingScript != null && sprintingScript.IsSprinting();
        // Optional: Add aiming state check for animation if needed
        // bool isAimingState = shootingScript != null && shootingScript.IsAiming;

        float targetAnimSpeed = idleAnimationSpeed;
        if (isCurrentlyMovingInput)
        {
            // Optional: Add specific animation speed logic for aiming if desired
            // if (isAimingState) targetAnimSpeed = aimingAnimationSpeed; else
            targetAnimSpeed = isSprintingState ? sprintAnimationMultiplier : normalAnimationSpeed;
        }

        // Apply the target speed
        dolphinAnimation[swimAnimationName].speed = targetAnimSpeed;

        // Ensure the designated animation is playing
        if (!dolphinAnimation.isPlaying)
        {
            dolphinAnimation.Play(swimAnimationName);
        }
        else if (!dolphinAnimation.IsPlaying(swimAnimationName))
        {
            dolphinAnimation.CrossFade(swimAnimationName, 0.1f);
        }
    }

    void UpdateAudio()
    {
        // Use input state for audio triggers as well
        bool isMovingHorizontallyInput = moveInput.magnitude > 0.1f;
        bool isMovingVerticallyInput = Mathf.Abs(verticalInput) > 0.1f;

        HandleAudioSource(movementSound, isMovingHorizontallyInput);
        HandleAudioSource(verticalMovementSound, isMovingVerticallyInput);
    }

    // Helper to manage looping audio sources
    void HandleAudioSource(AudioSource source, bool shouldBePlaying)
    {
        if (source == null) return;

        if (shouldBePlaying && !source.isPlaying)
        {
            source.Play();
        }
        else if (!shouldBePlaying && source.isPlaying)
        {
            source.Stop();
        }
    }

    // --- Movement Freeze Control (Used by CubeInteraction etc.) ---
    /// <summary>
    /// Freezes or unfreezes the dolphin's movement by controlling the Rigidbody and script state.
    /// </summary>
    /// <param name="freeze">True to freeze movement, false to unfreeze.</param>
    public void SetMovementFreeze(bool freeze)
    {
        isMovementFrozen = freeze; // Update internal state first

        if (rb == null) rb = GetComponent<Rigidbody>(); // Ensure rb is valid

        if (freeze)
        {
            // Stop existing physics movement and make it ignore forces
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true; // Stops physics simulation
            }
            // Stop movement sounds immediately
            HandleAudioSource(movementSound, false);
            HandleAudioSource(verticalMovementSound, false);
            // Reset input state to prevent leftover movement on unfreeze
            moveInput = Vector3.zero;
            verticalInput = 0f;
        }
        else
        {
            // Resume physics simulation
            if (rb != null)
            {
                rb.isKinematic = false; // Allow physics forces again
                                        // Optional slight delay or check before fully enabling input/forces if needed
            }
        }
        // Note: We don't disable the component anymore, we just block Update/FixedUpdate logic
        // based on the isMovementFrozen flag. This ensures SetMovementFreeze(false) can always be called.
    }

    /// <summary>
    /// Returns true if the movement script is currently NOT frozen.
    /// </summary>
    public bool IsMovementEnabled()
    {
        return !isMovementFrozen;
    }
}