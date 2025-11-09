using UnityEngine;

public class SloMo : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode slowMoKey = KeyCode.F; // hold to slow-mo

    [Header("Time Scale")]
    [SerializeField] private float slowTimeScale = 0.2f; // 20% speed
    private const float normalTimeScale = 1f;
    private float baseFixedDelta;

    [Header("Camera FOV while slow-mo")]
    [SerializeField] public float slowMoFov = 60f;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource; // 2D source for whooshes
    [SerializeField] private AudioClip enterSfx;     // plays when entering slo-mo
    [SerializeField] private AudioClip exitSfx;      // plays when exiting slo-mo
    [SerializeField] private float sfxVolume = 1f;

    public static bool IsActive { get; private set; }
    public static float CurrentPitchScale { get; private set; } = 1f; // 1 when normal, slowTimeScale when active

    // expose the active slow-mo FOV for the camera to read
    public static float ActiveSlowMoFov { get; private set; } = 60f;

    // Event so audio followers can update once on change
    public static System.Action<float> OnSlowMoPitchChanged; // passes new scale (1..slowTimeScale)

    private void Awake()
    {
        baseFixedDelta = Time.fixedDeltaTime;
        IsActive = false;
        CurrentPitchScale = 1f;

        // Sync the static FOV with the inspector value
        ActiveSlowMoFov = slowMoFov;

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f; // 2D
        }

    }

    // keep static in sync when edited in Inspector
    private void OnValidate()
    {
        if (!Application.isPlaying)
            ActiveSlowMoFov = slowMoFov;
    }

    private void Update()
    {
        bool holding = Input.GetKey(slowMoKey);

        if (holding && !IsActive)
        {
            // enter slow-mo
            IsActive = true;
            Time.timeScale = slowTimeScale;
            Time.fixedDeltaTime = baseFixedDelta * slowTimeScale;

            CurrentPitchScale = slowTimeScale;
            OnSlowMoPitchChanged?.Invoke(CurrentPitchScale);

            // NEW: play enter whoosh
            if (enterSfx && sfxSource) sfxSource.PlayOneShot(enterSfx, sfxVolume);
        }
        else if (!holding && IsActive)
        {
            // exit slow-mo
            IsActive = false;
            Time.timeScale = normalTimeScale;
            Time.fixedDeltaTime = baseFixedDelta;

            CurrentPitchScale = 1f;
            OnSlowMoPitchChanged?.Invoke(CurrentPitchScale);

            // NEW: play exit whoosh
            if (exitSfx && sfxSource) sfxSource.PlayOneShot(exitSfx, sfxVolume);
        }

    }

    private void OnDisable()
    {
        // safety reset
        IsActive = false;
        Time.timeScale = normalTimeScale;
        Time.fixedDeltaTime = baseFixedDelta;

        CurrentPitchScale = 1f;
        OnSlowMoPitchChanged?.Invoke(CurrentPitchScale);
    }
}
