using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemySpriteStateController : MonoBehaviour
{
    public enum State { Advancing, Engaging }
    private enum PulseType { None, Hit, Attack }

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Advancing (Walk) — Two Frames (Normal)")]
    [SerializeField] private Sprite walkFrameA;
    [SerializeField] private Sprite walkFrameB;
    [SerializeField] private float walkFps = 8f;

    [Header("Advancing (Walk) — Two Frames (Low HP)")]
    [SerializeField] private Sprite lowWalkFrameA;
    [SerializeField] private Sprite lowWalkFrameB;

    [Header("Engaging (Normal)")]
    [SerializeField] private Sprite engagingFrame;

    [Header("Engaging (Low HP)")]
    [SerializeField] private Sprite lowEngagingFrame;

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
    [SerializeField] private float lowHealthThreshold = 0.40f;

    private State state = State.Advancing;
    private PulseType pulse = PulseType.None;

    private Coroutine pulseRoutine;
    private Coroutine walkRoutine;

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

        ApplyBaseState();
    }

    private void OnDisable()
    {
        if (damageReceiver != null)
            damageReceiver.onHit.RemoveListener(OnHit);

        StopAllCoroutines();
        pulseRoutine = null;
        walkRoutine = null;
    }

    public void ShowAdvancing() { SetState(State.Advancing); }
    public void ShowEngaging() { SetState(State.Engaging); }
    public void PulseAttackFollowThrough() { StartPulse(PulseType.Attack, attackPulseSeconds); }

    private void OnHit(float dmg, WeaponData.DamageType type) { StartPulse(PulseType.Hit, hitPulseSeconds); }

    private void SetState(State next)
    {
        state = next;
        if (IsPulsing()) return;
        ApplyBaseState();
    }

    private void ApplyBaseState()
    {
        if (state == State.Advancing) StartWalking();
        else ShowEngagingIdle();
    }

    private bool IsPulsing() { return pulse != PulseType.None; }

    private void StartPulse(PulseType pulseType, float seconds)
    {
        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        pulse = pulseType;

        StopWalking();
        SetSprite(SelectPulseSprite(pulseType));

        pulseRoutine = StartCoroutine(PulseTimer(seconds));
    }

    private IEnumerator PulseTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        pulse = PulseType.None;
        pulseRoutine = null;
        ApplyBaseState();
    }

    private Sprite SelectPulseSprite(PulseType pulseType)
    {
        bool lowHp = IsLowHp();

        if (pulseType == PulseType.Attack)
            return lowHp ? lowAttackFollowThroughFrame : attackFollowThroughFrame;

        if (pulseType == PulseType.Hit)
            return lowHp ? lowHitFrame : hitFrame;

        return null;
    }

    private void StartWalking()
    {
        if (walkRoutine != null) return;
        walkRoutine = StartCoroutine(WalkFlipbook());
    }

    private void StopWalking()
    {
        if (walkRoutine == null) return;
        StopCoroutine(walkRoutine);
        walkRoutine = null;
    }

    private IEnumerator WalkFlipbook()
    {
        float interval = walkFps > 0f ? 1f / walkFps : 0.125f;
        WaitForSeconds wait = new WaitForSeconds(interval);

        bool useB = false;

        while (true)
        {
            bool low = IsLowHp();
            Sprite frameA = low ? lowWalkFrameA : walkFrameA;
            Sprite frameB = low ? lowWalkFrameB : walkFrameB;

            SetSprite(useB ? frameB : frameA);
            useB = !useB;
            yield return wait;
        }
    }

    private void ShowEngagingIdle()
    {
        StopWalking();
        bool low = IsLowHp();
        SetSprite(low ? lowEngagingFrame : engagingFrame);
    }

    private void SetSprite(Sprite frame)
    {
        if (spriteRenderer != null && frame != null)
            spriteRenderer.sprite = frame;
    }

    private bool IsLowHp()
    {
        if (damageReceiver == null) return false;
        return damageReceiver.GetHealthFraction() < lowHealthThreshold;
    }

    public void ShowSingleFrame(Sprite frame)
    {
        StopAllCoroutines();
        pulse = PulseType.None;
        walkRoutine = null;
        pulseRoutine = null;
        state = State.Engaging;
        SetSprite(frame);
    }
}
