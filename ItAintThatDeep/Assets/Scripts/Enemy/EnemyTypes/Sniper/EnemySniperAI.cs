using UnityEngine;

/*
Simple sniper AI:
- Finds the player if not assigned (Camera.main fallback).
- Faces the player on Y.
- Moves until within sniperRange, then stops.
- After engageWarmup, fires on cooldown toward the player's last known point.
- Notifies spriteState for advancing/engaging and plays follow-through pulse on shot.
*/
public class EnemySniperAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Sniper Behavior")]
    [SerializeField] private float sniperRange = 60f;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float turnSpeed = 720f;

    [Header("Warm-up")]
    [SerializeField] private float engageWarmup = 0.5f;
    private float warmupUntil = 0f;

    [Header("Firing")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private float fireCooldown = 1.25f;

    [Header("Optional Visuals")]
    [SerializeField] private EnemySpriteStateController spriteState;

    private float nextFireTime = 0f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // optional
    }

    private void Update()
    {
        if (EnsurePlayer() == false)
        {
            return;
        }

        Vector3 toPlayer = player.position - transform.position;
        Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        float dist = toPlayer.magnitude;

        Face(flat);

        if (dist > sniperRange)
        {
            warmupUntil = 0f;

            if (spriteState != null) spriteState.ShowAdvancing();

            Vector3 step = flat.normalized * moveSpeed * Time.deltaTime;
            if (rb != null && rb.isKinematic == false)
            {
                rb.MovePosition(rb.position + step);
            }
            else
            {
                transform.position = transform.position + step;
            }

            return;
        }

        if (warmupUntil == 0f)
        {
            warmupUntil = Time.time + engageWarmup;
        }

        if (Time.time < warmupUntil)
        {
            if (spriteState != null) spriteState.ShowEngaging();
            return;
        }

        if (Time.time >= nextFireTime)
        {
            Fire();
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
        {
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
        {
            return;
        }

        Quaternion target = Quaternion.LookRotation(flatToPlayer.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
    }

    private void Fire()
    {
        if (projectilePrefab == null)
        {
            return;
        }
        if (muzzle == null)
        {
            return;
        }
        if (player == null)
        {
            return;
        }

        Vector3 targetPoint = player.position;
        Vector3 dir = (targetPoint - muzzle.position).normalized;

        EnemyProjectile proj = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(dir));
        proj.Init(transform, dir);
    }
}
