using System.Collections;
using UnityEngine;

/*
Camera kick, shake, and FOV punch on weapon fire/hit and when the player is damaged.
*/
public class CameraKickAndShake : MonoBehaviour
{
    [Header("Player HIT (when the player takes damage)")]
    [SerializeField] private float playerHitPitch = 2.0f;
    [SerializeField] private float playerHitAmp = 0.05f;
    [SerializeField] private float playerHitFov = 3.0f;
    [SerializeField] private float playerHitStop = 0.03f;

    [Header("Durations")]
    [SerializeField] private float kickTime = 0.06f;
    [SerializeField] private float shakeTime = 0.08f;
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

    private Quaternion baseRot;
    private Vector3 baseLocalPos;
    private float baseFov;

    private static CameraKickAndShake instance;

    private void Awake()
    {
        instance = this;

        if (cam == null) cam = GetComponent<Camera>();

        baseRot = transform.localRotation;
        baseLocalPos = transform.localPosition;
        baseFov = cam != null ? cam.fieldOfView : 90f;

        var playerDamage = FindFirstObjectByType<PlayerHealthController>()?.GetComponent<DamageReciever>();
        if (playerDamage != null)
            playerDamage.onHit.AddListener(OnPlayerHit);
    }

    private void OnDestroy()
    {
        var playerDamage = FindFirstObjectByType<PlayerHealthController>()?.GetComponent<DamageReciever>();
        if (playerDamage != null)
            playerDamage.onHit.RemoveListener(OnPlayerHit);
    }

    public static void Fire(WeaponData.WeaponType type)
    {
        if (instance == null) return;

        float pitch, amp, fov;
        instance.Map(type, false, out pitch, out amp, out fov);
        instance.Play(pitch, amp, fov);
    }

    public static void Hit(WeaponData.WeaponType type)
    {
        if (instance == null) return;

        float pitch, amp, fov;
        instance.Map(type, true, out pitch, out amp, out fov);
        instance.Play(pitch, amp, fov);
    }

    private void OnPlayerHit(float amount, WeaponData.DamageType type)
    {
        if (playerHitStop > 0f) HitStop.Do(playerHitStop);
        Play(playerHitPitch, playerHitAmp, playerHitFov);
    }

    private void Map(WeaponData.WeaponType weaponType, bool isHit, out float pitch, out float amp, out float fov)
    {
        pitch = 1f; amp = 0.02f; fov = 2f;
        switch (weaponType)
        {
            case WeaponData.WeaponType.Melee:
            case WeaponData.WeaponType.Slap: pitch = meleeFirePitch; amp = meleeFireAmp; fov = meleeFireFov; break;
            case WeaponData.WeaponType.Pistol: pitch = pistolFirePitch; amp = pistolFireAmp; fov = pistolFireFov; break;
            case WeaponData.WeaponType.Rifle: pitch = rifleFirePitch; amp = rifleFireAmp; fov = rifleFireFov; break;
            case WeaponData.WeaponType.RPG: pitch = rpgFirePitch; amp = rpgFireAmp; fov = rpgFireFov; break;
            default: pitch = pistolFirePitch; amp = pistolFireAmp; fov = pistolFireFov; break;
        }
        if (isHit) { pitch *= hitPitchScale; amp *= hitAmpScale; fov *= hitFovScale; }
    }

    private void Play(float pitch, float amp, float fovKick)
    {
        // Sprinting? Nudge shake a bit stronger.
        if (FirstPersonController.IsSprinting)
            amp *= sprintShakeScale;

        StopAllCoroutines();
        StartCoroutine(KickCoroutine(pitch, kickTime));
        StartCoroutine(ShakeCoroutine(amp, shakeTime));
        if (cam != null) StartCoroutine(FovPunchCoroutine(fovKick, fovTime));
    }

    private IEnumerator KickCoroutine(float pitch, float time) { yield break; }
    private IEnumerator ShakeCoroutine(float amp, float time) { yield break; }
    private IEnumerator FovPunchCoroutine(float fovKick, float time) { yield break; }
}
