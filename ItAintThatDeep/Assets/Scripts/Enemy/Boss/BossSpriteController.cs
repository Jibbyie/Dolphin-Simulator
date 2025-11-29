// BossSpriteController.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BossSpriteController : MonoBehaviour
{
    public enum BossVisualStage { Stage1, Stage2 }

    [Header("General")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Stage 1 - Walk (Normal)")]
    [SerializeField] private Sprite s1WalkA;
    [SerializeField] private Sprite s1WalkB;
    [SerializeField] private float s1WalkFps = 8f;

    [Header("Stage 1 - Walk (Low HP)")]
    [SerializeField] private Sprite s1LowWalkA;
    [SerializeField] private Sprite s1LowWalkB;

    [Header("Stage 1 - Engaging")]
    [SerializeField] private Sprite s1Engage;
    [SerializeField] private Sprite s1EngageLow;

    [Header("Stage 1 - Attack / Hit")]
    [SerializeField] private Sprite s1Attack;
    [SerializeField] private Sprite s1AttackLow;
    [SerializeField] private Sprite s1Hit;
    [SerializeField] private Sprite s1HitLow;

    [Header("Stage 1 Special")]
    [SerializeField] private Sprite s1Drink;
    [SerializeField] private Sprite s1Transform;

    [Header("Stage 1 Low HP Threshold")]
    [SerializeField] private float lowHpThreshold = 0.4f;

    [Header("Stage 2 - Walk")]
    [SerializeField] private Sprite s2WalkA;
    [SerializeField] private Sprite s2WalkB;
    [SerializeField] private float s2WalkFps = 8f;

    [Header("Stage 2 - Punches")]
    [SerializeField] private Sprite s2PunchLeft;
    [SerializeField] private Sprite s2PunchRight;

    [Header("Stage 2 - Hit")]
    [SerializeField] private Sprite s2Hit;

    [Header("Timings")]
    [SerializeField] private float pulseSeconds = 0.2f;

    private BossVisualStage currentStage = BossVisualStage.Stage1;

    private Coroutine walkRoutine;
    private Coroutine pulseRoutine;

    private bool useWalkB;
    private bool useWalkB2;
    private bool useLeftPunch;

    private DamageReciever damageReceiver;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        damageReceiver = GetComponentInParent<DamageReciever>();
    }

    public void SetStage(BossVisualStage st)
    {
        currentStage = st;
        StopAllCoroutines();
        walkRoutine = null;
        pulseRoutine = null;
    }

    // -------------------------------------------------------
    // PULSE SYSTEM (shared)
    // -------------------------------------------------------
    private void StartPulse(Sprite frame)
    {
        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        StopWalk();
        spriteRenderer.sprite = frame;
        pulseRoutine = StartCoroutine(PulseTimer());
    }

    private IEnumerator PulseTimer()
    {
        yield return new WaitForSeconds(pulseSeconds);
        pulseRoutine = null;

        if (currentStage == BossVisualStage.Stage1)
            Stage1_ResumeState();
        else
            Stage2_ResumeState();
    }

    private void StopWalk()
    {
        if (walkRoutine != null)
            StopCoroutine(walkRoutine);
        walkRoutine = null;
    }

    // -------------------------------------------------------
    // STAGE 1
    // -------------------------------------------------------
    public void Stage1_ShowAdvancing()
    {
        if (currentStage != BossVisualStage.Stage1) return;

        if (pulseRoutine != null) return;

        if (walkRoutine == null)
            walkRoutine = StartCoroutine(Stage1WalkLoop());
    }

    public void Stage1_ShowEngaging()
    {
        if (currentStage != BossVisualStage.Stage1) return;
        if (pulseRoutine != null) return;

        StopWalk();
        bool low = IsLow();
        spriteRenderer.sprite = low ? s1EngageLow : s1Engage;
    }

    public void Stage1_PulseAttack()
    {
        bool low = IsLow();
        StartPulse(low ? s1AttackLow : s1Attack);
    }

    public void Stage1_PulseHit()
    {
        bool low = IsLow();
        StartPulse(low ? s1HitLow : s1Hit);
    }

    public void Stage1_PlayDrink()
    {
        StopAllCoroutines();
        walkRoutine = null;
        pulseRoutine = null;
        spriteRenderer.sprite = s1Drink;
    }

    public void Stage1_PlayTransform()
    {
        StopAllCoroutines();
        walkRoutine = null;
        pulseRoutine = null;
        spriteRenderer.sprite = s1Transform;
    }

    private IEnumerator Stage1WalkLoop()
    {
        float interval = s1WalkFps > 0 ? 1f / s1WalkFps : 0.125f;
        WaitForSeconds wait = new WaitForSeconds(interval);

        while (true)
        {
            bool low = IsLow();
            Sprite a = low ? s1LowWalkA : s1WalkA;
            Sprite b = low ? s1LowWalkB : s1WalkB;

            spriteRenderer.sprite = useWalkB ? b : a;
            useWalkB = !useWalkB;

            yield return wait;
        }
    }

    private void Stage1_ResumeState()
    {
        if (currentStage != BossVisualStage.Stage1) return;

        StopWalk();
        walkRoutine = StartCoroutine(Stage1WalkLoop());
    }

    // -------------------------------------------------------
    // STAGE 2
    // -------------------------------------------------------
    public void Stage2_ShowAdvancing()
    {
        if (currentStage != BossVisualStage.Stage2) return;
        if (pulseRoutine != null) return;

        if (walkRoutine == null)
            walkRoutine = StartCoroutine(Stage2WalkLoop());
    }

    public void Stage2_ShowPunch()
    {
        if (currentStage != BossVisualStage.Stage2) return;

        Sprite punch = useLeftPunch ? s2PunchLeft : s2PunchRight;
        useLeftPunch = !useLeftPunch;

        StartPulse(punch);
    }

    public void Stage2_ShowHit()
    {
        if (currentStage != BossVisualStage.Stage2) return;
        StartPulse(s2Hit);
    }

    private IEnumerator Stage2WalkLoop()
    {
        float interval = s2WalkFps > 0 ? 1f / s2WalkFps : 0.125f;
        WaitForSeconds wait = new WaitForSeconds(interval);

        while (true)
        {
            spriteRenderer.sprite = useWalkB2 ? s2WalkB : s2WalkA;
            useWalkB2 = !useWalkB2;
            yield return wait;
        }
    }

    private void Stage2_ResumeState()
    {
        if (currentStage != BossVisualStage.Stage2) return;

        StopWalk();
        walkRoutine = StartCoroutine(Stage2WalkLoop());
    }

    // -------------------------------------------------------
    private bool IsLow()
    {
        return damageReceiver != null &&
               damageReceiver.GetHealthFraction() < lowHpThreshold;
    }
}
