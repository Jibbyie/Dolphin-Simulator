using UnityEngine;

/*
Simple melee AI:
- Finds the player if not assigned.
- Faces the player on Y.
- Rushes until within punchRange.
- On entering range, waits engageWarmup, then punches on cooldown.
- Notifies spriteState for advancing/engaging and plays follow-through pulse on hit.
*/
public class EnemyMeleeAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

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
        float dist = flat.magnitude;

        Face(flat);

        if (dist > punchRange)
        {
            if (spriteState != null) spriteState.ShowAdvancing();

            warmupUntil = 0f;

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

        if (Time.time >= nextPunchTime)
        {
            TryPunch();
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
        if (player != null)
        {
            return true;
        }

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
        {
            return;
        }

        Quaternion target = Quaternion.LookRotation(flatToPlayer.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
    }

    private void TryPunch()
    {
        if (player == null)
        {
            return;
        }

        DamageReciever dr;
        bool has = player.TryGetComponent<DamageReciever>(out dr);
        if (has == true)
        {
            dr.RecieveDamage(punchDamage, WeaponData.DamageType.Melee);
            return;
        }

        var drInParent = player.GetComponentInParent<DamageReciever>();
        if (drInParent != null)
        {
            drInParent.RecieveDamage(punchDamage, WeaponData.DamageType.Melee);
        }
    }
}
