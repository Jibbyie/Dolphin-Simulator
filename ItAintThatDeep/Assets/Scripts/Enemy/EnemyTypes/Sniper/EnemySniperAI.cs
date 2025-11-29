using UnityEngine;

public class EnemySniperAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Activation")]
    [SerializeField] private float activationRadius = 30f;
    private bool activated = false;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float turnSpeed = 720f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 60f;
    [SerializeField] private float engageWarmup = 0.5f;
    [SerializeField] private float fireCooldown = 1.25f;
    [SerializeField] private Transform muzzle;
    [SerializeField] private EnemyProjectile projectilePrefab;

    [Header("Optional Visuals")]
    [SerializeField] private EnemySpriteStateController spriteState;

    private float warmupUntil = 0f;
    private float nextFireTime = 0f;

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

        // --------- Activation Check ---------
        if (!activated)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            if (distToPlayer <= activationRadius)
                activated = true;
            else
                return; // Stay idle until triggered
        }
        // ------------------------------------

        Vector3 toPlayer = player.position - transform.position;
        Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        float dist = flat.magnitude;

        Face(flat);

        if (dist > attackRange)
        {
            if (spriteState != null) spriteState.ShowAdvancing();
            warmupUntil = 0f;

            Vector3 step = flat.normalized * moveSpeed * Time.deltaTime;
            if (rb != null && !rb.isKinematic)
                rb.MovePosition(rb.position + step);
            else
                transform.position += step;

            return;
        }

        if (warmupUntil == 0f)
            warmupUntil = Time.time + engageWarmup;

        if (Time.time < warmupUntil)
        {
            if (spriteState != null) spriteState.ShowEngaging();
            return;
        }

        if (Time.time >= nextFireTime)
        {
            TryShoot();
            if (spriteState != null) spriteState.PulseAttackFollowThrough();
            nextFireTime = Time.time + fireCooldown;
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

    private void TryShoot()
    {
        if (projectilePrefab == null || muzzle == null || player == null) return;

        Vector3 targetPoint = player.position;
        Vector3 dir = (targetPoint - muzzle.position).normalized;

        EnemyProjectile proj = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(dir));
        proj.Init(transform, dir);

        // play gunshot attack sound
        GetComponent<EnemyAudioController>()?.PlayAttackSound();
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
