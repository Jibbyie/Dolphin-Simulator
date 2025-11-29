using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class FishSwimAI : MonoBehaviour
{
    [Header("Swimming")]
    [SerializeField] private float baseSwimSpeed = 2.5f;
    [SerializeField] private float speedVariation = 1f;
    [SerializeField] private float turnSpeed = 2f;
    [SerializeField] private float directionChangeInterval = 3f;
    [SerializeField] private float maxSwimRadius = 20f;

    [Header("Bobbing")]
    [SerializeField] private float bobAmplitude = 0.5f;
    [SerializeField] private float bobFrequency = 1.2f;

    [Header("Audio")]
    [SerializeField] private AudioClip[] swimSounds;
    [SerializeField] private float soundIntervalMin = 4f;
    [SerializeField] private float soundIntervalMax = 12f;

    [Header("Sprite Child")]
    [SerializeField] private Transform spriteChild;

    private Vector3 homePosition;
    private Quaternion targetRotation;
    private float nextDirectionTime;
    private float baseY;
    private AudioSource audioSource;

    private float currentSpeed;
    private float targetSpeed;

    private Transform playerCam;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (spriteChild == null)
            spriteChild = GetComponentInChildren<SpriteRenderer>().transform;

        playerCam = Camera.main.transform;
    }

    private void Start()
    {
        homePosition = transform.position;
        baseY = transform.position.y;

        PickRandomRotation();
        PickRandomSpeed();
        ScheduleNextSound();

        nextDirectionTime = Time.time + directionChangeInterval;
        currentSpeed = baseSwimSpeed;
    }

    private void Update()
    {
        HandleRotation();
        SwimForward();
        UpdateSpeed();
        Bobbing();
        OrientSprite();
    }

    // ----------------------------
    // Movement
    // ----------------------------

    private void HandleRotation()
    {
        if (Time.time >= nextDirectionTime)
        {
            PickRandomRotation();
            PickRandomSpeed();
            nextDirectionTime = Time.time + directionChangeInterval;
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }

    private void SwimForward()
    {
        transform.position += transform.forward * currentSpeed * Time.deltaTime;

        if ((transform.position - homePosition).sqrMagnitude > maxSwimRadius * maxSwimRadius)
        {
            Vector3 backHome = (homePosition - transform.position).normalized;
            targetRotation = Quaternion.LookRotation(backHome, Vector3.up);
        }
    }

    private void UpdateSpeed()
    {
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 0.7f);
    }

    private void Bobbing()
    {
        float newY = baseY + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        Vector3 pos = transform.position;
        pos.y = newY;
        transform.position = pos;
    }

    // ----------------------------
    // Sprite Orientation
    // ----------------------------

    private void OrientSprite()
    {
        if (spriteChild == null) return;

        // Always face camera (billboard)
        Vector3 camPos = playerCam.position;
        Vector3 lookDir = camPos - spriteChild.position;
        lookDir.y = 0; // flatten so fish doesn't roll weirdly

        if (lookDir.sqrMagnitude > 0.001f)
        {
            spriteChild.rotation = Quaternion.LookRotation(-lookDir, Vector3.up);
        }

        // Rotate toward swimming direction (yaw only)
        Vector3 forward = transform.forward;
        forward.y = 0;

        if (forward.sqrMagnitude > 0.001f)
        {
            Quaternion swimRot = Quaternion.LookRotation(forward, Vector3.up);
            spriteChild.rotation = Quaternion.Lerp(spriteChild.rotation, swimRot, Time.deltaTime * 4f);
        }
    }

    // ----------------------------
    // Randomization
    // ----------------------------

    private void PickRandomRotation()
    {
        Vector3 randomDir = Random.insideUnitSphere;
        randomDir.y *= 0.4f;
        randomDir.Normalize();

        targetRotation = Quaternion.LookRotation(randomDir, Vector3.up);
    }

    private void PickRandomSpeed()
    {
        targetSpeed = baseSwimSpeed + Random.Range(-speedVariation, speedVariation);
        if (targetSpeed < 0.2f) targetSpeed = 0.2f;
    }

    // ----------------------------
    // Audio
    // ----------------------------

    private void ScheduleNextSound()
    {
        StartCoroutine(SoundRoutine());
    }

    private IEnumerator SoundRoutine()
    {
        while (true)
        {
            float wait = Random.Range(soundIntervalMin, soundIntervalMax);
            yield return new WaitForSeconds(wait);

            if (swimSounds != null && swimSounds.Length > 0)
            {
                AudioClip clip = swimSounds[Random.Range(0, swimSounds.Length)];
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
