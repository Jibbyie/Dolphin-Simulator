using System.Collections;
using UnityEngine;

/*
Controls enemy 2D sprite visuals with normal and low-HP variants.

States:
- Advancing: two-frame walk flipbook at walkFps
- Engaging: single "ready/charge" sprite

Temporary pulses:
- Attack follow-through pulse
- Hit pulse

Low-HP variants:
- Two advancing frames
- One engaging frame
- One attack follow-through frame
- One hit frame
*/
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
    [SerializeField]
    [Tooltip("Below this fraction, use the low-HP sprites")]
    private float lowHealthThreshold = 0.40f;

    private State state = State.Advancing;
    private PulseType pulse = PulseType.None;

    private Coroutine pulseRoutine;
    private Coroutine walkRoutine;

    private DamageReciever damageReceiver;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        damageReceiver = GetComponent<DamageReciever>();
    }

    private void OnEnable()
    {
        if (damageReceiver != null)
        {
            damageReceiver.onHit.AddListener(OnHit);
        }

        ApplyBaseState();
    }

    private void OnDisable()
    {
        if (damageReceiver != null)
        {
            damageReceiver.onHit.RemoveListener(OnHit);
        }

        StopAllCoroutines();
        pulseRoutine = null;
        walkRoutine = null;
    }

    // Called by AI when moving toward the player
    public void ShowAdvancing()
    {
        SetState(State.Advancing);
    }

    // Called by AI when in melee range but waiting to attack
    public void ShowEngaging()
    {
        SetState(State.Engaging);
    }

    // Called by AI right when a punch happens (brief pulse)
    public void PulseAttackFollowThrough()
    {
        StartPulse(PulseType.Attack, attackPulseSeconds);
    }

    private void OnHit(float damage, WeaponData.DamageType type)
    {
        StartPulse(PulseType.Hit, hitPulseSeconds);
    }

    private void SetState(State next)
    {
        state = next;

        if (IsPulsing() == true)
        {
            return;
        }

        ApplyBaseState();
    }

    private void ApplyBaseState()
    {
        if (state == State.Advancing)
        {
            StartWalking();
        }
        else
        {
            ShowEngagingIdle();
        }
    }

    private bool IsPulsing()
    {
        if (pulse == PulseType.None)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void StartPulse(PulseType pulseType, float seconds)
    {
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
        }

        pulse = pulseType;

        StopWalking();

        Sprite frame = SelectPulseSprite(pulseType);
        SetSprite(frame);

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
        {
            if (lowHp == true)
            {
                if (lowAttackFollowThroughFrame != null)
                {
                    return lowAttackFollowThroughFrame;
                }
            }

            return attackFollowThroughFrame;
        }

        if (pulseType == PulseType.Hit)
        {
            if (lowHp == true)
            {
                if (lowHitFrame != null)
                {
                    return lowHitFrame;
                }
            }

            return hitFrame;
        }

        return null;
    }

    private void StartWalking()
    {
        if (walkRoutine != null)
        {
            return;
        }

        walkRoutine = StartCoroutine(WalkFlipbook());
    }

    private void StopWalking()
    {
        if (walkRoutine == null)
        {
            return;
        }

        StopCoroutine(walkRoutine);
        walkRoutine = null;
    }

    private IEnumerator WalkFlipbook()
    {
        float interval;
        if (walkFps > 0f)
        {
            interval = 1f / walkFps;
        }
        else
        {
            interval = 0.125f;
        }

        WaitForSeconds wait = new WaitForSeconds(interval);

        bool useB = false;

        while (true)
        {
            Sprite frameA = walkFrameA;
            Sprite frameB = walkFrameB;

            bool lowHp = IsLowHp();

            if (lowHp == true)
            {
                if (lowWalkFrameA != null)
                {
                    frameA = lowWalkFrameA;
                }
                if (lowWalkFrameB != null)
                {
                    frameB = lowWalkFrameB;
                }
            }

            if (useB == true)
            {
                SetSprite(frameB);
            }
            else
            {
                SetSprite(frameA);
            }

            useB = !useB;

            yield return wait;
        }
    }

    private void ShowEngagingIdle()
    {
        StopWalking();

        bool lowHp = IsLowHp();

        if (lowHp == true)
        {
            if (lowEngagingFrame != null)
            {
                SetSprite(lowEngagingFrame);
                return;
            }
        }

        SetSprite(engagingFrame);
    }

    private void SetSprite(Sprite frame)
    {
        if (spriteRenderer != null)
        {
            if (frame != null)
            {
                spriteRenderer.sprite = frame;
            }
        }
    }

    private bool IsLowHp()
    {
        float fraction = GetHealthFractionSafe();

        if (fraction < lowHealthThreshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private float GetHealthFractionSafe()
    {
        if (damageReceiver == null)
        {
            return 1f;
        }

        float value = damageReceiver.GetHealthFraction();
        if (value < 0f) value = 0f;
        if (value > 1f) value = 1f;
        return value;
    }
}
