using UnityEngine;
using UnityEngine.UI; // Make sure you have 'using UnityEngine.UI;'

[RequireComponent(typeof(DolphinMovement))]
public class Sprinting : MonoBehaviour
{
    [Header("Sprinting Settings")]
    public float maxSprint = 100f;
    public float sprintDecreaseRate = 10f;
    public float sprintRecoveryRate = 15f;
    [Tooltip("Multiplier applied to the base movement force when sprinting.")]
    public float sprintMultiplier = 2.0f; // This is read by DolphinMovement
    public float minSprintToStart = 20f; // Minimum stamina needed to *begin* sprinting
    public float minSprintToMaintain = 5f; // Stamina level below which sprinting stops

    [Header("UI")]
    public Text sprintText; // Drag the UI text here

    [Header("Audio")]
    public AudioSource sprintSound; // Assign sprinting audio (should be looping)

    private float currentSprint;
    private bool isSprinting = false;
    private DolphinMovement movementScript; // Reference for checking movement state if needed

    void Start()
    {
        currentSprint = maxSprint;
        movementScript = GetComponent<DolphinMovement>();

        if (movementScript == null)
        {
            Debug.LogError("Sprinting script requires DolphinMovement script on the same GameObject.", this);
        }

        UpdateSprintUI(); // Initial UI update
    }

    void Update()
    {
        // Check movement input (using DolphinMovement's input state might be slightly cleaner, but this works)
        // We need to know if the player *intends* to move forward/sideways to allow sprinting
        bool wantsToMove = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f;

        // Conditions to be sprinting: Holding Shift, wants to move, has enough stamina OR is already sprinting and has minimum stamina
        bool canSprintNow = Input.GetKey(KeyCode.LeftShift) && wantsToMove &&
                           ((isSprinting && currentSprint > minSprintToMaintain) || (!isSprinting && currentSprint > minSprintToStart));

        if (canSprintNow)
        {
            // --- Start or Continue Sprinting ---
            if (!isSprinting)
            {
                isSprinting = true;
                // Play sound only when starting
                if (sprintSound != null)
                {
                    sprintSound.Play();
                }
            }

            currentSprint -= sprintDecreaseRate * Time.deltaTime;
            currentSprint = Mathf.Max(currentSprint, 0); // Ensure it doesn't go below 0

            // Check if we ran out of stamina while sprinting
            if (currentSprint <= minSprintToMaintain)
            {
                StopSprinting();
            }
        }
        else
        {
            // --- Stop Sprinting or Recover Stamina ---
            if (isSprinting)
            {
                StopSprinting(); // Stop if Shift released, stopped moving, or ran out
            }

            // Recover stamina if not currently trying to sprint (or stopped)
            if (!Input.GetKey(KeyCode.LeftShift) || !wantsToMove || currentSprint < maxSprint)
            {
                currentSprint += sprintRecoveryRate * Time.deltaTime;
                currentSprint = Mathf.Min(currentSprint, maxSprint); // Ensure it doesn't exceed max
            }
        }

        UpdateSprintUI();
    }

    void StopSprinting()
    {
        if (!isSprinting) return; // Already stopped

        isSprinting = false;
        if (sprintSound != null)
        {
            sprintSound.Stop(); // Or fade out
        }
    }

    void UpdateSprintUI()
    {
        if (sprintText != null)
        {
            // Using F0 format specifier to show whole number
            sprintText.text = $"Sprint: {currentSprint:F0} / {maxSprint:F0}";
        }
    }

    // Public getter for DolphinMovement to know the state
    public bool IsSprinting()
    {
        return isSprinting;
    }

    // Public getter for DolphinMovement to get the multiplier
    public float GetSprintMultiplier()
    {
        // Return 1 if not sprinting to avoid multiplying force by 0 if script disabled etc.
        return isSprinting ? sprintMultiplier : 1.0f;
    }
}