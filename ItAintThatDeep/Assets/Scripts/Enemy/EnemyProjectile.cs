using UnityEngine;

/*
Handles a straight-line projectile:

• Init(...) sets the shooter root to ignore and the initial forward direction.
• Moves forward at "speed" until "lifetime" expires or it hits something.
• On hit:
  - Ignores the shooter (and their children).
  - Applies damage to DamageReciever if present.
  - Spawns optional hit VFX.
  - Despawns the projectile.
*/
public class EnemyProjectile : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 5f;

    [Header("Damage")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private WeaponData.DamageType damageType = WeaponData.DamageType.Rifle;

    [Header("FX")]
    [SerializeField] private GameObject hitVfx;

    // Root transform of the shooter; used to prevent self-collision and child collisions
    private Transform shooterRoot;

    /*
    Initializes the projectile with a shooter to ignore and a travel direction.
    Direction is normalized before use.
    */
    public void Init(Transform ignoreRoot, Vector3 direction)
    {
        shooterRoot = ignoreRoot;

        Vector3 dir = direction;
        if (dir.sqrMagnitude > 0f)
        {
            dir = dir.normalized;
        }
        else
        {
            dir = transform.forward;
        }

        transform.forward = dir;
    }

    private void OnEnable()
    {
        // Safety despawn in case nothing is hit
        Invoke(nameof(Despawn), lifetime);
    }

    private void OnDisable()
    {
        // Ensure no stray invokes remain if object is disabled early
        CancelInvoke();
    }

    private void Update()
    {
        // Constant-velocity forward motion in world space
        Vector3 delta = transform.forward * speed * Time.deltaTime;
        transform.position = transform.position + delta;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore collisions with the shooter and anything under them
        if (shooterRoot != null)
        {
            bool hitShooter = other.transform.IsChildOf(shooterRoot);
            if (hitShooter == true)
            {
                return;
            }
        }

        // Apply damage if the hit object supports it
        DamageReciever damageReciever;
        bool hasReceiver = other.TryGetComponent<DamageReciever>(out damageReciever);
        if (hasReceiver == true)
        {
            damageReciever.RecieveDamage(damage, damageType);
        }

        // Spawn hit VFX at impact point
        if (hitVfx != null)
        {
            Instantiate(hitVfx, transform.position, Quaternion.identity);
        }

        Despawn();
    }

    /*
    Cancels timers and destroys the projectile.
    */
    private void Despawn()
    {
        CancelInvoke();
        Destroy(gameObject);
    }
}
