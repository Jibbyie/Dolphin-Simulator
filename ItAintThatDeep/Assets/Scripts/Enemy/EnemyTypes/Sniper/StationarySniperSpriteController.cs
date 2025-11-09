using System.Collections;
using UnityEngine;

/*
Stationary Sniper Sprite Controller
----------------------------------
- Only supports Idle (engaging), Attack follow-through, and Hit pulses.
- No advancing/walking logic.
- Keeps low-HP variants and timing identical to EnemySpriteStateController.
*/

[RequireComponent(typeof(SpriteRenderer))]
public class StationarySniperSpriteController : MonoBehaviour
{
    private enum PulseType { None, Hit, Attack }

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Idle (Normal)")]
    [SerializeField] private Sprite idleFrame;

    [Header("Idle (Low HP)")]
    [SerializeField] private Sprite lowIdleFrame;

    [Header("Attack / Hit Override Sprites (Normal)")]
    [SerializeField] private Sprite attackFollowThroughFrame;
    [SerializeField] private Sprite hitFrame;

    [Header("Attack / Hit Override Sprites (Low HP)")]
    [SerializeField] private Sprite lowAttackFollowThroughFrame;
    [SerializeField] private Sprite lowHitFrame;

    [Header("Timings")]
    [SerializeField] private float attackPulseSeconds = 0.20f;
    [SerializeField] private float hitPulseSeconds = 0.10f;

    [Header("Low-HP Threshold")]
    [SerializeField, Tooltip("Below this fraction, use the low-HP sprites")]
    private float lowHealthThreshold = 0.40f;

    private PulseType pulse = PulseType.None;
    private Coroutine pulseRoutine;
    private DamageReciever damageReceiver;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        damageReceiver = GetComponent<DamageReciever>();
    }

    private void OnEnable()
    {
        if (damageReceiver != null)
            damageReceiver.onHit.AddListener(OnHit);

        ApplyIdle();
    }

    private void OnDisable()
    {
        if (damageReceiver != null)
            damageReceiver.onHit.RemoveListener(OnHit);

        StopAllCoroutines();
        pulseRoutine = null;
    }

    // Called by AI to show idle/engaging (aiming)
    public void ShowEngaging()
    {
        if (IsPulsing())
            return;

        ApplyIdle();
    }

    // Called by AI on attack
    public void PulseAttackFollowThrough()
    {
        StartPulse(PulseType.Attack, attackPulseSeconds);
    }

    private void OnHit(float damage, WeaponData.DamageType type)
    {
        StartPulse(PulseType.Hit, hitPulseSeconds);
    }

    private bool IsPulsing() => pulse != PulseType.None;

    private void StartPulse(PulseType type, float seconds)
    {
        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        pulse = type;

        Sprite frame = SelectPulseSprite(type);
        SetSprite(frame);

        pulseRoutine = StartCoroutine(PulseTimer(seconds));
    }

    private IEnumerator PulseTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        pulse = PulseType.None;
        pulseRoutine = null;
        ApplyIdle();
    }

    private void ApplyIdle()
    {
        bool lowHp = IsLowHp();
        SetSprite(lowHp && lowIdleFrame != null ? lowIdleFrame : idleFrame);
    }

    private Sprite SelectPulseSprite(PulseType type)
    {
        bool lowHp = IsLowHp();

        if (type == PulseType.Attack)
            return lowHp && lowAttackFollowThroughFrame != null ? lowAttackFollowThroughFrame : attackFollowThroughFrame;

        if (type == PulseType.Hit)
            return lowHp && lowHitFrame != null ? lowHitFrame : hitFrame;

        return null;
    }

    private void SetSprite(Sprite frame)
    {
        if (spriteRenderer != null && frame != null)
            spriteRenderer.sprite = frame;
    }

    private bool IsLowHp()
    {
        float f = GetHealthFractionSafe();
        return f < lowHealthThreshold;
    }

    private float GetHealthFractionSafe()
    {
        if (damageReceiver == null)
            return 1f;

        float val = damageReceiver.GetHealthFraction();
        return Mathf.Clamp01(val);
    }
}
