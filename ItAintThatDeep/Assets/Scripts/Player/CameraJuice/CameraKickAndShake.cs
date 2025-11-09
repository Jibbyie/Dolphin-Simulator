using System.Collections;
using UnityEngine;

/*
Camera kick, shake, and FOV punch — purely visual.
Never interferes with the real aim ray.
*/
[DisallowMultipleComponent]
public class CameraKickAndShake : MonoBehaviour
{
    [Header("Player HIT (when the player takes damage)")]
    [SerializeField] private float playerHitPitch = 2.0f;
    [SerializeField] private float playerHitAmp = 0.05f;
    [SerializeField] private float playerHitFov = 3.0f;
    [SerializeField] private float playerHitStop = 0.03f;

    [Header("Durations")]
    [SerializeField] private float kickTime = 0.06f;
    [SerializeField] private float shakeTime = 0.25f;
    [SerializeField] private float fovTime = 0.10f;

    [Header("Base FIRE magnitudes per weapon")]
    [SerializeField] private float pistolFirePitch = 1.2f;
    [SerializeField] private float pistolFireAmp = 0.02f;
    [SerializeField] private float pistolFireFov = 2.0f;

    [SerializeField] private float meleeFirePitch = 1.2f;
    [SerializeField] private float meleeFireAmp = 0.02f;
    [SerializeField] private float meleeFireFov = 2.0f;

    [SerializeField] private float rifleFirePitch = 2.0f;
    [SerializeField] private float rifleFireAmp = 0.035f;
    [SerializeField] private float rifleFireFov = 3.5f;

    [SerializeField] private float rpgFirePitch = 3.0f;
    [SerializeField] private float rpgFireAmp = 0.06f;
    [SerializeField] private float rpgFireFov = 6.0f;

    [Header("HIT multipliers (applied on top of FIRE values)")]
    [SerializeField] private float hitPitchScale = 1.5f;
    [SerializeField] private float hitAmpScale = 1.5f;
    [SerializeField] private float hitFovScale = 1.2f;

    [Header("Sprint")]
    [SerializeField] private float sprintShakeScale = 1.25f;

    [Header("Refs")]
    [SerializeField] private Camera cam;

    private static CameraKickAndShake instance;

    // State
    private Vector3 kickOffset;
    private Vector3 shakeOffset;
    private float shakeAmplitude;
    private float shakeTimer;
    private float baseFov;
    private float targetFov;

    private void Awake()
    {
        instance = this;
        if (cam == null) cam = GetComponent<Camera>();
        if (cam != null) baseFov = cam.fieldOfView;

        var playerDamage = FindFirstObjectByType<PlayerHealthController>()?.GetComponent<DamageReciever>();
        if (playerDamage != null)
            playerDamage.onHit.AddListener(OnPlayerHit);
    }

    private void Start()
    {
        kickOffset = Vector3.zero;
        shakeOffset = Vector3.zero;
        targetFov = baseFov;
        if (cam != null) cam.fieldOfView = baseFov;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
    }


    private void OnDestroy()
    {
        var playerDamage = FindFirstObjectByType<PlayerHealthController>()?.GetComponent<DamageReciever>();
        if (playerDamage != null)
            playerDamage.onHit.RemoveListener(OnPlayerHit);
    }

    private void LateUpdate()
    {
        // Smoothly decay shake
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float fade = Mathf.Clamp01(shakeTimer / shakeTime);
            float amp = shakeAmplitude * fade;
            shakeOffset = new Vector3(
                (Mathf.PerlinNoise(Time.time * 20f, 0f) - 0.5f) * amp,
                (Mathf.PerlinNoise(0f, Time.time * 20f) - 0.5f) * amp,
                0f);
        }
        else shakeOffset = Vector3.zero;

        // Apply final local offset (visual only)
        transform.localRotation = Quaternion.Euler(kickOffset);
        transform.localPosition = shakeOffset;

        // Smooth FOV recovery
        if (cam != null)
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * 8f);
    }

    public static void Fire(WeaponData.WeaponType type)
    {
        if (instance == null) return;
        instance.Play(type, false);
    }

    public static void Hit(WeaponData.WeaponType type)
    {
        if (instance == null) return;
        instance.Play(type, true);
    }

    private void OnPlayerHit(float _, WeaponData.DamageType __)
    {
        if (playerHitStop > 0f) HitStop.Do(playerHitStop);
        PlayRaw(playerHitPitch, playerHitAmp, playerHitFov);
    }

    private void Play(WeaponData.WeaponType type, bool isHit)
    {
        float pitch, amp, fov;
        Map(type, isHit, out pitch, out amp, out fov);
        PlayRaw(pitch, amp, fov);
    }

    private void PlayRaw(float pitch, float amp, float fovKick)
    {
        if (FirstPersonController.IsSprinting)
            amp *= sprintShakeScale;

        StopAllCoroutines();
        StartCoroutine(KickRoutine(pitch));
        StartCoroutine(ShakeRoutine(amp));
        StartCoroutine(FovRoutine(fovKick));
    }

    private void Map(WeaponData.WeaponType weaponType, bool isHit, out float pitch, out float amp, out float fov)
    {
        pitch = pistolFirePitch; amp = pistolFireAmp; fov = pistolFireFov;
        switch (weaponType)
        {
            case WeaponData.WeaponType.Melee:
            case WeaponData.WeaponType.Slap: pitch = meleeFirePitch; amp = meleeFireAmp; fov = meleeFireFov; break;
            case WeaponData.WeaponType.Pistol: pitch = pistolFirePitch; amp = pistolFireAmp; fov = pistolFireFov; break;
            case WeaponData.WeaponType.Rifle: pitch = rifleFirePitch; amp = rifleFireAmp; fov = rifleFireFov; break;
            case WeaponData.WeaponType.RPG: pitch = rpgFirePitch; amp = rpgFireAmp; fov = rpgFireFov; break;
        }
        if (isHit) { pitch *= hitPitchScale; amp *= hitAmpScale; fov *= hitFovScale; }
    }

    private IEnumerator KickRoutine(float pitch)
    {
        float elapsed = 0f;
        float duration = kickTime;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float ease = 1f - Mathf.Pow(1f - t, 2f); // ease out
            kickOffset = new Vector3(-pitch * (1f - ease), 0f, 0f);
            yield return null;
        }

        // Smooth return
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            kickOffset = new Vector3(Mathf.Lerp(-pitch, 0f, t), 0f, 0f);
            yield return null;
        }
        kickOffset = Vector3.zero;
    }

    private IEnumerator ShakeRoutine(float amp)
    {
        shakeAmplitude = amp;
        shakeTimer = shakeTime;
        yield return new WaitForSeconds(shakeTime);
        shakeAmplitude = 0f;
    }

    private IEnumerator FovRoutine(float fovKick)
    {
        if (cam == null) yield break;

        float start = baseFov;
        float peak = baseFov + fovKick;
        targetFov = peak;

        float elapsed = 0f;
        float dur = fovTime * 0.5f;

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            cam.fieldOfView = Mathf.Lerp(start, peak, elapsed / dur);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            cam.fieldOfView = Mathf.Lerp(peak, baseFov, elapsed / dur);
            yield return null;
        }

        targetFov = baseFov;
    }
}
