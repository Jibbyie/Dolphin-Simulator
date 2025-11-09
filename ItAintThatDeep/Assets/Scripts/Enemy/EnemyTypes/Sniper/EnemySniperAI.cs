using UnityEngine;

/*
Ranged enemy AI:
- Finds the player if not assigned.
- Faces the player on Y.
- Moves until within attackRange.
- On entering range, waits engageWarmup, then fires projectiles on cooldown.
- Notifies spriteState for advancing/engaging and plays follow-through pulse on shot.
*/
public class EnemySniperAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // optional
    }

    private void Update()
    {
        if (!EnsurePlayer())
            return;

        Vector3 toPlayer = player.position - transform.position;
        Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        float dist = flat.magnitude;

        Face(flat);

        // Move toward player if too far
        if (dist > attackRange)
        {
            if (spriteState != null) spriteState.ShowAdvancing();

            warmupUntil = 0f;

            Vector3 step = flat.normalized * moveSpeed * Time.deltaTime;
            if (rb != null && rb.isKinematic == false)
                rb.MovePosition(rb.position + step);
            else
                transform.position += step;

            return;
        }

        // Begin warmup once within range
        if (warmupUntil == 0f)
            warmupUntil = Time.time + engageWarmup;

        // Still warming up
        if (Time.time < warmupUntil)
        {
            if (spriteState != null) spriteState.ShowEngaging();
            return;
        }

        // Fire on cooldown
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
        if (player != null)
            return true;

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
        if (flatToPlayer.sqrMagnitude <= 0.0001f)
            return;

        Quaternion target = Quaternion.LookRotation(flatToPlayer.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
    }

    private void TryShoot()
    {
        if (projectilePrefab == null || muzzle == null || player == null)
            return;

        Vector3 targetPoint = player.position;
        Vector3 dir = (targetPoint - muzzle.position).normalized;

        EnemyProjectile proj = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(dir));
        proj.Init(transform, dir);
    }
}
