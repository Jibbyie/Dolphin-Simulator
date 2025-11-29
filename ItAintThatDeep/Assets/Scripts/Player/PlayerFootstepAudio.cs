using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SloMoAudio))]
public class PlayerFootstepAudio : MonoBehaviour
{
    [Header("Footstep Clips")]
    [SerializeField] private AudioClip[] walkClips;
    [SerializeField] private AudioClip[] sprintClips;

    [Header("Jump Clips")]
    [SerializeField] private AudioClip[] jumpClips;

    [Header("Timing")]
    [SerializeField] private float walkStepInterval = 0.45f;
    [SerializeField] private float sprintStepInterval = 0.3f;

    [Header("Pitch Randomization")]
    [SerializeField] private Vector2 walkPitchRange = new Vector2(0.9f, 1.1f);
    [SerializeField] private Vector2 sprintPitchRange = new Vector2(1.1f, 1.3f);
    [SerializeField] private Vector2 jumpPitchRange = new Vector2(0.85f, 1.15f);

    private AudioSource src;
    private FirstPersonController controller;

    private bool wasGrounded;
    private float stepTimer = 0f;

    private void Awake()
    {
        src = GetComponent<AudioSource>();
        controller = GetComponent<FirstPersonController>();
        wasGrounded = FirstPersonController.IsGroundedNow;
        stepTimer = 0f;
    }

    private void Update()
    {
        bool grounded = FirstPersonController.IsGroundedNow;
        bool sprinting = FirstPersonController.IsSprinting;

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        bool moving = Mathf.Abs(inputX) + Mathf.Abs(inputZ) > 0.1f;

        // ------------------------
        // JUMP SOUND
        // ------------------------
        if (wasGrounded && !grounded)
        {
            PlayJumpClip();
        }

        wasGrounded = grounded;

        // ------------------------
        // FOOTSTEPS
        // ------------------------
        if (!grounded || !moving)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            if (sprinting)
            {
                PlaySprintStep();
                stepTimer = sprintStepInterval;
            }
            else
            {
                PlayWalkStep();
                stepTimer = walkStepInterval;
            }
        }
    }

    private void PlayWalkStep()
    {
        if (walkClips.Length == 0) return;
        src.pitch = Random.Range(walkPitchRange.x, walkPitchRange.y);
        src.PlayOneShot(walkClips[Random.Range(0, walkClips.Length)]);
    }

    private void PlaySprintStep()
    {
        if (sprintClips.Length == 0) return;
        src.pitch = Random.Range(sprintPitchRange.x, sprintPitchRange.y);
        src.PlayOneShot(sprintClips[Random.Range(0, sprintClips.Length)]);
    }

    private void PlayJumpClip()
    {
        if (jumpClips.Length == 0) return;
        src.pitch = Random.Range(jumpPitchRange.x, jumpPitchRange.y);
        src.PlayOneShot(jumpClips[Random.Range(0, jumpClips.Length)]);
    }
}
