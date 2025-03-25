using UnityEngine;
using UnityEngine.UI;

public class BreathingSystem : MonoBehaviour
{
    [Header("Breathing Settings")]
    public float maxBreath = 100f;
    public float breathDecreaseRate = 5f;
    public float breathRecoveryRate = 20f; // Recovery speed when at surface
    public float waterSurfaceHeight = 10f;

    [Header("UI")]
    public Text breathText; // Drag the UI text here

    [Header("Audio")]
    public AudioSource breathLossSound; // Continuous sound when losing breath
    public AudioSource breathRecoverySound; // Continuous sound when recovering breath

    private float currentBreath;
    private bool isBreathDecreasing = false;
    private bool isBreathRecovering = false;

    void Start()
    {
        currentBreath = maxBreath; // Start at full breath
    }

    void Update()
    {
        if (transform.position.y < waterSurfaceHeight) // Underwater
        {
            DecreaseBreath();
        }
        else if (transform.position.y >= waterSurfaceHeight) // At surface
        {
            RecoverBreath();
        }

        UpdateBreathUI();
    }

    void DecreaseBreath()
    {
        currentBreath -= breathDecreaseRate * Time.deltaTime;
        currentBreath = Mathf.Clamp(currentBreath, 0, maxBreath); // Prevent negative breath

        // Play breath loss sound
        if (!isBreathDecreasing)
        {
            isBreathDecreasing = true;
            if (breathLossSound != null && !breathLossSound.isPlaying)
            {
                breathLossSound.Play();
            }
        }

        // Stop recovery sound if playing
        if (isBreathRecovering && breathRecoverySound != null)
        {
            breathRecoverySound.Stop();
            isBreathRecovering = false;
        }
    }

    void RecoverBreath()
    {
        currentBreath += breathRecoveryRate * Time.deltaTime;
        currentBreath = Mathf.Clamp(currentBreath, 0, maxBreath); // Prevent exceeding max

        // Play recovery sound only if breath isn't full
        if (!isBreathRecovering && currentBreath < maxBreath)
        {
            isBreathRecovering = true;
            if (breathRecoverySound != null && !breathRecoverySound.isPlaying)
            {
                breathRecoverySound.Play();
            }
        }

        // Stop breath loss sound if playing
        if (isBreathDecreasing && breathLossSound != null)
        {
            breathLossSound.Stop();
            isBreathDecreasing = false;
        }
    }

    void UpdateBreathUI()
    {
        if (breathText != null)
        {
            breathText.text = "Breath: " + Mathf.CeilToInt(currentBreath) + "/100";
        }
    }

    public float GetCurrentBreath()
    {
        return currentBreath;
    }
}
