using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DolphinMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float floatSpeed = 2f;
    public float sinkSpeed = 2f;
    public float rotationSpeed = 10f;
    public float waterSurfaceHeight = 10f;
    public float normalAnimationSpeed = 1.5f;
    public float sprintAnimationMultiplier = 3f;

    [Header("Aim Settings")]
    public float aimingMoveSpeedMultiplier = 0.7f; // Slower movement while aiming

    [Header("Audio")]
    public AudioSource movementSound;
    public AudioSource floatingSound;
    public AudioSource sinkingSound;

    // Private variables
    private Rigidbody rb;
    private Animation dolphinAnimation;
    private Sprinting sprintingScript;
    private Transform cameraTransform;
    private DolphinShooting shootingScript;

    // Input variables
    private Vector2 movementInput;
    private bool isFloatingInput;
    private bool isSinkingInput;
    private bool isAimingInput;

    // State variables
    private bool isMoving = false;
    private bool isFloating = false;
    private bool isSinking = false;
    private bool isAiming = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        dolphinAnimation = GetComponentInChildren<Animation>();
        sprintingScript = GetComponent<Sprinting>();
        shootingScript = GetComponent<DolphinShooting>();

        // Find the main camera's transform
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Main camera not found! Ensure there is a camera tagged as MainCamera.");
        }

        if (dolphinAnimation == null)
        {
            Debug.LogError("Dolphin Animation component not found! Make sure it's on the child object.");
        }
    }

    void Update()
    {
        // Get input from controller/keyboard
        GetInput();

        // Update animation
        UpdateAnimation();

        // Handle audio
        UpdateAudio();
    }

    void FixedUpdate()
    {
        // Apply movement
        ApplyMovement();

        // Apply vertical movement (floating/sinking)
        ApplyVerticalMovement();
    }

    void GetInput()
    {
        // Get movement input (works with both keyboard and controller)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        movementInput = new Vector2(horizontal, vertical);

        // Get floating/sinking input
        isFloatingInput = Input.GetButton("Jump"); // Space on keyboard, button on controller
        isSinkingInput = Input.GetKey(KeyCode.F); // F on keyboard, could map to controller button

        // Get aiming input (LT on controller, could be right mouse on keyboard)
        isAimingInput = Input.GetButton("Fire2"); // Right mouse or LT usually

        // Update aiming state
        if (isAimingInput != isAiming)
        {
            isAiming = isAimingInput;
            if (shootingScript != null)
            {
                shootingScript.SetAimingMode(isAiming);
            }
        }
    }

    void ApplyMovement()
    {
        if (cameraTransform == null) return;

        // Only proceed if we have movement input
        if (movementInput.magnitude > 0.1f)
        {
            isMoving = true;

            // Convert input to 3D space relative to camera
            Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward).normalized;

            // Calculate the direction in world space
            Vector3 moveDirection = (cameraForward * movementInput.y + cameraRight * movementInput.x).normalized;

            // Apply speed multiplier if aiming
            float currentMoveSpeed = moveSpeed;
            if (isAiming)
            {
                currentMoveSpeed *= aimingMoveSpeedMultiplier;
            }
            else if (sprintingScript != null && sprintingScript.IsSprinting())
            {
                currentMoveSpeed *= sprintingScript.sprintMultiplier;
            }

            // Apply movement
            Vector3 movement = moveDirection * currentMoveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);

            // Rotate to face movement direction if not aiming
            if (!isAiming)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
            }
        }
        else
        {
            isMoving = false;
        }
    }

    void ApplyVerticalMovement()
    {
        float verticalVelocity = 0f;

        // Handle floating
        if (isFloatingInput && transform.position.y < waterSurfaceHeight)
        {
            verticalVelocity = floatSpeed;
            isFloating = true;

            // Play floating sound if not already playing
            if (floatingSound != null && !floatingSound.isPlaying)
            {
                floatingSound.Play();
            }
        }
        // Handle sinking
        else if (isSinkingInput)
        {
            verticalVelocity = -sinkSpeed;
            isSinking = true;

            // Play sinking sound if not already playing
            if (sinkingSound != null && !sinkingSound.isPlaying)
            {
                sinkingSound.Play();
            }
        }
        else
        {
            isFloating = false;
            isSinking = false;

            // Stop floating and sinking sounds
            if (floatingSound != null && floatingSound.isPlaying)
            {
                floatingSound.Stop();
            }

            if (sinkingSound != null && sinkingSound.isPlaying)
            {
                sinkingSound.Stop();
            }
        }

        // Apply vertical movement
        rb.velocity = new Vector3(rb.velocity.x, verticalVelocity, rb.velocity.z);
    }

    void UpdateAnimation()
    {
        if (dolphinAnimation != null)
        {
            string animName = "Armature_Dolphin|Armature_Dolphin|Armature_Dolphin|Idle";
            if (dolphinAnimation[animName] != null)
            {
                if (!isMoving)
                {
                    dolphinAnimation[animName].speed = 1.0f; // Idle animation speed
                }
                else if (sprintingScript != null && sprintingScript.IsSprinting() && !isAiming)
                {
                    dolphinAnimation[animName].speed = sprintAnimationMultiplier; // Sprint animation
                }
                else if (isAiming)
                {
                    dolphinAnimation[animName].speed = normalAnimationSpeed * 0.8f; // Slower when aiming
                }
                else
                {
                    dolphinAnimation[animName].speed = normalAnimationSpeed; // Normal speed
                }
            }
            else
            {
                Debug.LogError("Animation clip '" + animName + "' not found!");
            }
        }
    }

    void UpdateAudio()
    {
        // Handle movement sound
        if (isMoving && !movementSound.isPlaying)
        {
            movementSound.Play();
        }
        else if (!isMoving && movementSound.isPlaying)
        {
            movementSound.Stop();
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Check if the collision is with a wall
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Get the contact point
            ContactPoint contact = collision.contacts[0];

            // Calculate push direction
            Vector3 pushBackDirection = transform.position - contact.point;
            pushBackDirection.Normalize();

            // Push back from wall
            rb.MovePosition(transform.position + pushBackDirection * 0.1f);

            // Zero out velocity component in collision direction
            Vector3 adjustedVelocity = Vector3.ProjectOnPlane(rb.velocity, contact.normal);
            rb.velocity = adjustedVelocity;

            // Zero out angular velocity
            rb.angularVelocity = Vector3.zero;
        }
    }

    // Public methods for other scripts to check state
    public bool IsAiming() { return isAiming; }
    public bool IsMoving() { return isMoving; }
}