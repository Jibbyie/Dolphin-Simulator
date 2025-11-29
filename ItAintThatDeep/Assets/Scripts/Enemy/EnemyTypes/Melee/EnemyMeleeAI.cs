using UnityEngine;

public class EnemyMeleeAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Activation")]
    [SerializeField] private float activationRadius = 30f;
    private bool activated = false;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float turnSpeed = 720f;

    [Header("Melee")]
    [SerializeField] private float punchRange = 2.0f;
    [SerializeField] private float punchCooldown = 0.8f;
    [SerializeField] private float punchDamage = 15f;
    [SerializeField] private float engageWarmup = 0.4f;

    [SerializeField] private EnemySpriteStateController spriteState;

    private float warmupUntil = 0f;
    private float nextPunchTime = 0f;
    private Rigidbody rb;
    private EnemyBaseAI stun;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stun = GetComponent<EnemyBaseAI>();
    }

    private void Update()
    {
        if (stun != null && stun.IsStunned) return;
        if (!EnsurePlayer()) return;

        if (!activated)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            if (distToPlayer <= activationRadius)
                activated = true;
            else
                return;
        }

        Vector3 toPlayer = player.position - transform.position;
        Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        float dist = flat.magnitude;

        Face(flat);

        if (dist > punchRange)
        {
            if (spriteState != null) spriteState.ShowAdvancing();
            warmupUntil = 0f;

            Vector3 step = flat.normalized * moveSpeed * Time.deltaTime;
            if (!rb.isKinematic)
                rb.MovePosition(rb.position + step);

            return;
        }

        if (warmupUntil == 0f)
            warmupUntil = Time.time + engageWarmup;

        if (Time.time < warmupUntil)
        {
            if (spriteState != null) spriteState.ShowEngaging();
            return;
        }

        if (Time.time >= nextPunchTime)
        {
            TryPunch();

            // play melee swing sound
            GetComponent<EnemyAudioController>()?.PlayAttackSound();

            if (spriteState != null) spriteState.PulseAttackFollowThrough();
            nextPunchTime = Time.time + punchCooldown;
        }

        else
        {
            if (spriteState != null) spriteState.ShowEngaging();
        }
    }

    private bool EnsurePlayer()
    {
        if (player != null) return true;

        var healthCtrl = FindFirstObjectByType<PlayerHealthController>();
        if (healthCtrl != null)
        {
            player = healthCtrl.transform;
            return true;
        }

        if (Camera.main != null)
        {
            player = Camera.main.transform;
            return true;
        }

        return false;
    }

    private void Face(Vector3 flatToPlayer)
    {
        if (flatToPlayer.sqrMagnitude <= 0.0001f) return;

        Quaternion target = Quaternion.LookRotation(flatToPlayer.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
    }

    private void TryPunch()
    {
        if (player == null) return;

        if (player.TryGetComponent(out DamageReciever dr))
        {
            dr.RecieveDamage(punchDamage, WeaponData.DamageType.Melee);
            return;
        }

        var drParent = player.GetComponentInParent<DamageReciever>();
        if (drParent != null)
            drParent.RecieveDamage(punchDamage, WeaponData.DamageType.Melee);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
