using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sprinting : MonoBehaviour
{
    [Header("Sprinting Settings")]
    public float maxSprint = 100f;
    public float sprintDecreaseRate = 10f;
    public float sprintRecoveryRate = 15f;
    public float sprintMultiplier = 2f;
    public float minSprintToSprint = 20f; // Minimum sprint needed to start sprinting

    [Header("UI")]
    public Text sprintText; // Drag the UI text here

    [Header("Audio")]
    public AudioSource sprintSound; // Assign sprinting audio

    private float currentSprint;
    private bool isSprinting = false;

    private DolphinMovement movementScript;
    private float defaultMoveSpeed; // Store the default movement speed

    void Start()
    {
        currentSprint = maxSprint;
        movementScript = GetComponent<DolphinMovement>(); // Get movement script

        // Store the original movement speed
        defaultMoveSpeed = movementScript.moveSpeed;
    }

    void Update()
    {
        bool isMoving = Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;

        // Check if player can start sprinting
        bool canSprint = Input.GetKey(KeyCode.LeftShift) && currentSprint > minSprintToSprint && isMoving;

        if (canSprint)
        {
            Sprint();
        }
        else
        {
            RecoverSprint();
        }

        UpdateSprintUI();
    }

    void Sprint()
    {
        if (!isSprinting)
        {
            isSprinting = true;
            movementScript.moveSpeed = defaultMoveSpeed * sprintMultiplier; // Only set once
        }

        currentSprint -= sprintDecreaseRate * Time.deltaTime;
        currentSprint = Mathf.Clamp(currentSprint, 0, maxSprint);

        // Play sprint sound
        if (sprintSound != null && !sprintSound.isPlaying)
        {
            sprintSound.Play();
        }

        // Stop sprint if we drop below minimum sprint requirement
        if (currentSprint <= minSprintToSprint)
        {
            isSprinting = false;
            movementScript.moveSpeed = defaultMoveSpeed;
        }
    }

    void RecoverSprint()
    {
        if (isSprinting && (currentSprint <= minSprintToSprint || !Input.GetKey(KeyCode.LeftShift)))
        {
            isSprinting = false;
            movementScript.moveSpeed = defaultMoveSpeed; // Reset speed only once
        }

        currentSprint += sprintRecoveryRate * Time.deltaTime;
        currentSprint = Mathf.Clamp(currentSprint, 0, maxSprint);

        // Stop sprint sound
        if (sprintSound != null)
        {
            sprintSound.Stop();
        }
    }

    void UpdateSprintUI()
    {
        if (sprintText != null)
        {
            sprintText.text = "Sprint: " + Mathf.CeilToInt(currentSprint) + "/100";
        }
    }

    public bool IsSprinting()
    {
        return isSprinting;
    }
}
