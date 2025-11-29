using UnityEngine;
using UnityEngine.Video;
using System.Collections;

[RequireComponent(typeof(DamageReciever))]
public class BossController : MonoBehaviour
{
    private enum BossStage { Stage1, Healing, Stage2 }

    [Header("Core")]
    [SerializeField] private DamageReciever damageReceiver;
    [SerializeField] private BossSpriteController spriteController;
    [SerializeField] private Transform player;

    [Header("Cutscenes")]
    [SerializeField] private VideoClip stage2Cutscene;

    [Header("Stage 1 (Ranged)")]
    [SerializeField] private float stage1MaxHP = 400f;
    [SerializeField] private float s1MoveSpeed = 3.5f;
    [SerializeField] private float s1TurnSpeed = 720f;
    [SerializeField] private float s1AttackRange = 60f;
    [SerializeField] private float s1EngageWarmup = 0.5f;
    [SerializeField] private float s1FireCooldown = 1.25f;
    [SerializeField] private Transform s1Muzzle;
    [SerializeField] private EnemyProjectile s1ProjectilePrefab;

    [Header("Stage 2 (Melee Boxing)")]
    [SerializeField] private float stage2MaxHP = 800f;
    [SerializeField] private float s2MoveSpeed = 6f;
    [SerializeField] private float s2TurnSpeed = 720f;
    [SerializeField] private float s2PunchRange = 2.0f;
    [SerializeField] private float s2PunchCooldown = 0.5f;
    [SerializeField] private float s2PunchDamage = 30f;
    [SerializeField] private float stage2ScaleMultiplier = 2f;

    [Header("Special Timings")]
    [SerializeField] private float drinkDuration = 1.0f;
    [SerializeField] private float transformDuration = 1.2f;

    private BossStage stage = BossStage.Stage1;
    private bool healedOnce = false;
    private bool inSequence = false;

    private float s1WarmupUntil = 0f;
    private float s1NextFireTime = 0f;
    private float s2NextPunchTime = 0f;

    private Vector3 originalScale;
    private Rigidbody rb;
    private EnemyHealthTint healthTint;

    private EnemyBaseAI stun;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stun = GetComponent<EnemyBaseAI>();
        damageReceiver = GetComponent<DamageReciever>();
        healthTint = GetComponent<EnemyHealthTint>();

        if (player == null)
        {
            var hp = FindFirstObjectByType<PlayerHealthController>();
            if (hp != null) player = hp.transform;
        }

        originalScale = transform.localScale;

        SetMaxHP(stage1MaxHP);
        SetHealthToMax();

        spriteController.SetStage(BossSpriteController.BossVisualStage.Stage1);

        damageReceiver.onHit.AddListener(OnBossDamaged);
    }

    private void OnDestroy()
    {
        damageReceiver.onHit.RemoveListener(OnBossDamaged);
    }

    private void Update()
    {
        if (stun != null && stun.IsStunned) return;

        if (player == null)
        {
            var hp = FindFirstObjectByType<PlayerHealthController>();
            if (hp != null) player = hp.transform;
            if (player == null) return;
        }

        if (inSequence) return;

        float frac = damageReceiver.GetHealthFraction();

        if (stage == BossStage.Stage1)
        {
            if (!healedOnce && frac <= 0.5f)
            {
                StartCoroutine(HealSequence());
                return;
            }

            if (healedOnce && frac <= 0.5f)
            {
                StartCoroutine(Stage2Sequence());
                return;
            }

            UpdateStage1();
        }
        else if (stage == BossStage.Stage2)
        {
            UpdateStage2();
        }
    }

    private void UpdateStage1()
    {
        Vector3 flat = player.position - transform.position;
        flat.y = 0;
        float dist = flat.magnitude;

        Face(flat, s1TurnSpeed);

        if (dist > s1AttackRange)
        {
            spriteController.Stage1_ShowAdvancing();
            s1WarmupUntil = 0f;
            Move(flat.normalized * s1MoveSpeed * Time.deltaTime);
            return;
        }

        if (s1WarmupUntil == 0f)
            s1WarmupUntil = Time.time + s1EngageWarmup;

        if (Time.time < s1WarmupUntil)
        {
            spriteController.Stage1_ShowEngaging();
            return;
        }

        if (Time.time >= s1NextFireTime)
        {
            TryShootStage1();
            spriteController.Stage1_PulseAttack();
            s1NextFireTime = Time.time + s1FireCooldown;
        }
        else
        {
            spriteController.Stage1_ShowEngaging();
        }
    }

    private void TryShootStage1()
    {
        if (s1ProjectilePrefab == null || s1Muzzle == null)
            return;

        Vector3 dir = (player.position - s1Muzzle.position).normalized;

        EnemyProjectile proj = Instantiate(
            s1ProjectilePrefab,
            s1Muzzle.position,
            Quaternion.LookRotation(dir)
        );

        proj.Init(transform, dir);
    }

    private void UpdateStage2()
    {
        Vector3 flat = player.position - transform.position;
        flat.y = 0;
        float dist = flat.magnitude;

        Face(flat, s2TurnSpeed);

        if (dist > s2PunchRange)
        {
            spriteController.Stage2_ShowAdvancing();
            Move(flat.normalized * s2MoveSpeed * Time.deltaTime);
            return;
        }

        if (Time.time >= s2NextPunchTime)
        {
            spriteController.Stage2_ShowPunch();
            TryPunchStage2();
            s2NextPunchTime = Time.time + s2PunchCooldown;
        }
    }

    private void TryPunchStage2()
    {
        if (player.TryGetComponent(out DamageReciever dr))
        {
            dr.RecieveDamage(s2PunchDamage, WeaponData.DamageType.Melee);
            return;
        }

        var drParent = player.GetComponentInParent<DamageReciever>();
        if (drParent != null)
            drParent.RecieveDamage(s2PunchDamage, WeaponData.DamageType.Melee);
    }

    private IEnumerator HealSequence()
    {
        inSequence = true;
        stage = BossStage.Healing;

        spriteController.Stage1_PlayDrink();
        yield return new WaitForSeconds(drinkDuration);

        SetMaxHP(stage1MaxHP);
        SetHealthToMax();
        if (healthTint != null) healthTint.ApplyHealthTint();

        healedOnce = true;
        stage = BossStage.Stage1;
        inSequence = false;
    }

    private IEnumerator Stage2Sequence()
    {
        inSequence = true;
        stage = BossStage.Healing;

        spriteController.Stage1_PlayTransform();
        yield return new WaitForSeconds(transformDuration);

        // Existing transformation logic
        transform.localScale = originalScale * stage2ScaleMultiplier;

        damageReceiver.DamageTypeImmunities.Clear();
        damageReceiver.DamageTypeImmunities.Add(WeaponData.DamageType.Pistol);
        damageReceiver.DamageTypeImmunities.Add(WeaponData.DamageType.Rifle);
        damageReceiver.DamageTypeImmunities.Add(WeaponData.DamageType.Explosive);

        SetMaxHP(stage2MaxHP);
        SetHealthToMax();
        if (healthTint != null) healthTint.ApplyHealthTint();

        spriteController.SetStage(BossSpriteController.BossVisualStage.Stage2);

        stage = BossStage.Stage2;
        inSequence = false;
    }

    private void OnBossDamaged(float dmg, WeaponData.DamageType type)
    {
        if (stage == BossStage.Stage1)
            spriteController.Stage1_PulseHit();
        else if (stage == BossStage.Stage2)
            spriteController.Stage2_ShowHit();
    }

    private void Face(Vector3 flat, float turnSpeed)
    {
        if (flat.sqrMagnitude < 0.0001f)
            return;

        Quaternion target = Quaternion.LookRotation(flat.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
    }

    private void Move(Vector3 step)
    {
        if (rb && !rb.isKinematic)
            rb.MovePosition(rb.position + step);
        else
            transform.position += step;
    }

    private void SetMaxHP(float value)
    {
        var field = typeof(DamageReciever).GetField("maxHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(damageReceiver, value);
    }

    private void SetHealthToMax()
    {
        var current = typeof(DamageReciever).GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var max = typeof(DamageReciever).GetField("maxHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (current != null && max != null)
            current.SetValue(damageReceiver, (float)max.GetValue(damageReceiver));
    }
}
