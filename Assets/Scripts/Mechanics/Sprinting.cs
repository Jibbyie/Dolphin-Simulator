using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(DolphinMovement))]
public class Sprinting : MonoBehaviour
{
    [Header("Sprint Settings")]
    [Tooltip("Maximum sprint meter value")]
    public float maxSprintMeter = 100f;
    [Tooltip("How fast the sprint meter drains when sprinting")]
    public float drainRate = 10f;
    [Tooltip("How fast the sprint meter recovers when not sprinting")]
    public float recoveryRate = 15f;
    [Tooltip("Speed multiplier when sprinting")]
    public float sprintMultiplier = 2.0f;
    [Tooltip("Minimum sprint meter needed to start sprinting")]
    public float minToStartSprint = 20f;
    [Tooltip("Minimum sprint meter needed to continue sprinting")]
    public float minToMaintainSprint = 5f;

    [Header("UI")]
    public Text sprintMeterText;

    [Header("Audio")]
    public AudioSource sprintSound;

    // Internal state
    private float currentSprintMeter;
    private bool isSprinting = false;
    private DolphinMovement movementScript;

    void Start()
    {
        currentSprintMeter = maxSprintMeter;
        movementScript = GetComponent<DolphinMovement>();

        if (movementScript == null)
        {
            Debug.LogError("Sprint system requires DolphinMovement component", this);
            enabled = false;
        }

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateSprintUI();
    }

    void Update()
    {
        // Skip if movement is disabled
        if (movementScript != null && !movementScript.IsMovementEnabled())
        {
            if (isSprinting)
            {
                StopSprinting();
            }
            return;
        }

        // Ensure cursor remains locked during gameplay
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Check if player is trying to move
        bool isMoving = IsPlayerMoving();

        // Determine if player can sprint (using Left Shift key)
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift);
        bool canStartSprint = !isSprinting && currentSprintMeter > minToStartSprint;
        bool canMaintainSprint = isSprinting && currentSprintMeter > minToMaintainSprint;

        // Sprint when: wants to sprint, is moving, and either starting or maintaining sprint
        if (wantsToSprint && isMoving && (canStartSprint || canMaintainSprint))
        {
            // Start sprinting if not already
            if (!isSprinting)
            {
                isSprinting = true;

                // Play sprint sound
                if (sprintSound != null)
                {
                    sprintSound.Play();
                }
            }

            // Drain sprint meter
            currentSprintMeter -= drainRate * Time.deltaTime;
            currentSprintMeter = Mathf.Max(currentSprintMeter, 0f);

            // Stop sprinting if meter gets too low
            if (currentSprintMeter <= minToMaintainSprint)
            {
                StopSprinting();
            }
        }
        else
        {
            // Stop sprinting if we are sprinting but can't anymore
            if (isSprinting)
            {
                StopSprinting();
            }

            // Recover sprint meter when not trying to sprint
            if (!wantsToSprint || !isMoving)
            {
                currentSprintMeter += recoveryRate * Time.deltaTime;
                currentSprintMeter = Mathf.Min(currentSprintMeter, maxSprintMeter);
            }
        }

        // Update UI
        UpdateSprintUI();
    }

    bool IsPlayerMoving()
    {
        return Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f ||
               Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f;
    }

    void StopSprinting()
    {
        isSprinting = false;

        // Stop sprint sound
        if (sprintSound != null && sprintSound.isPlaying)
        {
            sprintSound.Stop();
        }
    }

    void UpdateSprintUI()
    {
        if (sprintMeterText != null)
        {
            sprintMeterText.text = $"Sprint: {Mathf.RoundToInt(currentSprintMeter)}/{Mathf.RoundToInt(maxSprintMeter)}";
        }
    }

    // Public getter to check if sprinting
    public bool IsSprinting()
    {
        return isSprinting;
    }

    // Public getter for the sprint multiplier
    public float GetSprintMultiplier()
    {
        return isSprinting ? sprintMultiplier : 1.0f;
    }
}