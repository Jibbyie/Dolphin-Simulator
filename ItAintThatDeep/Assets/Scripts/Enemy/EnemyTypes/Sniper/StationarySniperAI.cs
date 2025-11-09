using UnityEngine;

/*
Stationary sniper:
- Does NOT move.
- Faces the player on Y and keeps aiming.
- Has a larger default detection range than the moving sniper.
- Uses StationarySniperSpriteController for visuals (idle, attack follow-through, hit).
*/
public class StationarySniperAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Aiming")]
    [SerializeField] private float turnSpeed = 720f;

    [Header("Attack (stationary)")]
    [Tooltip("How far this stationary sniper will detect/engage the player.")]
    [SerializeField] private float attackRange = 120f; // larger default
    [SerializeField] private float engageWarmup = 0.5f;
    [SerializeField] private float fireCooldown = 1.25f;
    [SerializeField] private Transform muzzle;
    [SerializeField] private EnemyProjectile projectilePrefab;

    [Header("Visuals")]
    [SerializeField] private StationarySniperSpriteController spriteState;

    private float warmupUntil = 0f;
    private float nextFireTime = 0f;

    private void Update()
    {
        if (!EnsurePlayer())
            return;

        Vector3 toPlayer = player.position - transform.position;
        Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        float dist = flat.magnitude;

        // Always face the player (only yaw)
        Face(flat);

        // Out of range - just idle
        if (dist > attackRange)
        {
            if (spriteState != null)
                spriteState.ShowEngaging(); // idle equivalent
            warmupUntil = 0f;
            return;
        }

        // Start warmup if entering range
        if (warmupUntil == 0f)
            warmupUntil = Time.time + engageWarmup;

        // Still warming up
        if (Time.time < warmupUntil)
        {
            if (spriteState != null)
                spriteState.ShowEngaging();
            return;
        }

        // Ready to fire
        if (Time.time >= nextFireTime)
        {
            TryShoot();
            if (spriteState != null)
                spriteState.PulseAttackFollowThrough();

            nextFireTime = Time.time + fireCooldown;
        }
        else
        {
            if (spriteState != null)
                spriteState.ShowEngaging();
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

        Vector3 dir = (player.position - muzzle.position).normalized;
        EnemyProjectile proj = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(dir));
        proj.Init(transform, dir);
    }
}
