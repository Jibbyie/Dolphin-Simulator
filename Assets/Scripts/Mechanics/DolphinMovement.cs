using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DolphinMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float floatSpeed = 2f;
    public float sinkSpeed = 2f;
    public float rotationSpeed = 100f; // Speed of rotation
    public float waterSurfaceHeight = 10f; // Limit for floating
    public float normalAnimationSpeed = 1.5f; // Animation speed when moving normally
    public float sprintAnimationMultiplier = 3f; // Sprinting animation multiplier

    private Rigidbody rb;
    private Animation dolphinAnimation; // Reference to the Animation component
    private Sprinting sprintingScript; // Reference to sprinting script

    [Header("Audio")]
    public AudioSource movementSound;
    public AudioSource floatingSound;
    public AudioSource sinkingSound;

    private bool isMoving = false;
    private bool isFloating = false;
    private bool isSinking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Disable gravity for floating effect
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Prevent unwanted rotation

        dolphinAnimation = GetComponentInChildren<Animation>();
        sprintingScript = GetComponent<Sprinting>(); // Get reference to sprinting

        if (dolphinAnimation == null)
        {
            Debug.LogError("Dolphin Animation component not found! Make sure it's on the child object.");
        }
    }

    void FixedUpdate() // Use FixedUpdate for Rigidbody physics
    {
        // Forward & Backward movement (based on current rotation)
        float moveZ = Input.GetAxis("Vertical");
        isMoving = Mathf.Abs(moveZ) > 0.1f; // Check if moving

        if (isMoving)
        {
            Vector3 move = transform.forward * moveZ * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move); // Use Rigidbody.MovePosition()
        }

        // Rotation (A & D keys for turning)
        float rotateY = Input.GetAxis("Horizontal") * rotationSpeed * Time.fixedDeltaTime;
        Quaternion deltaRotation = Quaternion.Euler(0, rotateY, 0);
        rb.MoveRotation(rb.rotation * deltaRotation); // Use Rigidbody.MoveRotation()

        // Play movement sound
        if (isMoving && !movementSound.isPlaying)
        {
            movementSound.Play();
        }
        else if (!isMoving && movementSound.isPlaying)
        {
            movementSound.Stop();
        }

        // Adjust animation speed based on movement & sprinting
        if (dolphinAnimation != null)
        {
            string animName = "Armature_Dolphin|Armature_Dolphin|Armature_Dolphin|Idle";
            if (dolphinAnimation[animName] != null)
            {
                bool isSprinting = sprintingScript != null && sprintingScript.IsSprinting();
                if (!isMoving)
                {
                    dolphinAnimation[animName].speed = 1.0f; // Idle animation speed
                }
                else if (isSprinting)
                {
                    dolphinAnimation[animName].speed = sprintAnimationMultiplier; // Faster animation when sprinting
                }
                else
                {
                    dolphinAnimation[animName].speed = normalAnimationSpeed; // Normal movement speed
                }
            }
            else
            {
                Debug.LogError("Animation clip '" + animName + "' not found in the Animation component.");
            }
        }

        // Floating & Sinking
        float verticalVelocity = 0f;

        if (Input.GetKey(KeyCode.Space) && transform.position.y < waterSurfaceHeight) // Floating up
        {
            verticalVelocity = floatSpeed;
            if (!isFloating)
            {
                isFloating = true;
                if (floatingSound != null && !floatingSound.isPlaying)
                {
                    floatingSound.Play();
                }
            }
        }
        else if (Input.GetKey(KeyCode.F)) // Sinking down
        {
            verticalVelocity = -sinkSpeed;
            if (!isSinking)
            {
                isSinking = true;
                if (sinkingSound != null && !sinkingSound.isPlaying)
                {
                    sinkingSound.Play();
                }
            }
        }
        else
        {
            isFloating = false;
            isSinking = false;
            if (floatingSound != null)
            {
                floatingSound.Stop();
            }
            if (sinkingSound != null)
            {
                sinkingSound.Stop();
            }
        }

        // Apply vertical movement
        rb.velocity = new Vector3(rb.velocity.x, verticalVelocity, rb.velocity.z);
    }

    void OnCollisionStay(Collision collision) // PROTOTYPE HARD CODED FIX TO STOP PHASING THROUGH WALLS
    {
        // Check if the collision is with a wall
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Get the contact point of the collision
            ContactPoint contact = collision.contacts[0];

            // Calculate the pushback direction
            Vector3 pushBackDirection = transform.position - contact.point;
            pushBackDirection.Normalize();

            // Push the dolphin back slightly from the wall
            rb.MovePosition(transform.position + pushBackDirection * 0.1f);

            // Stop all forward movement to prevent further penetration
            Vector3 adjustedVelocity = rb.velocity;
            adjustedVelocity.z = 0;
            rb.velocity = adjustedVelocity;

            // Zero out angular velocity to prevent rotation from collision
            rb.angularVelocity = Vector3.zero;
        }
    }


}