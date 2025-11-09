using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SloMoAudio : MonoBehaviour
{
    [Header("Mapping")]
    [SerializeField] private bool matchTimeScale = true; // true = use scale from controller
    [SerializeField] private float slowPitchOverride = 0.2f; // used if matchTimeScale == false

    private AudioSource src;
    private float basePitch;

    private void Awake()
    {
        src = GetComponent<AudioSource>();
        basePitch = src.pitch;
    }

    private void OnEnable()
    {
        // Subscribe to slow-mo updates
        SloMo.OnSlowMoPitchChanged += ApplyScale;

        // Sync immediately to current state
        ApplyScale(SloMo.CurrentPitchScale);
    }

    private void OnDisable()
    {
        SloMo.OnSlowMoPitchChanged -= ApplyScale;

        // Restore original pitch
        if (src != null) src.pitch = basePitch;
    }

    private void ApplyScale(float scaleFromController)
    {
        float scale = matchTimeScale ? scaleFromController : slowPitchOverride;
        src.pitch = basePitch * Mathf.Clamp(scale, 0.01f, 3f);
    }
}
