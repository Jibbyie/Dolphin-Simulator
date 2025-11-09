using UnityEngine;

/*
Simplified projectile:
- Flies straight toward the player (no tilt).
- Moves at constant speed until lifetime expires or hits something.
*/
public class EnemyProjectile : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;

    [Header("Damage")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private WeaponData.DamageType damageType = WeaponData.DamageType.Rifle;

    [Header("FX")]
    [SerializeField] private GameObject hitVfx;

    private Transform shooterRoot;
    private Vector3 direction;

    public void Init(Transform ignoreRoot, Vector3 dir)
    {
        shooterRoot = ignoreRoot;

        // Force normalized, level direction (no downward aim)
        dir.y = 0f;
        direction = dir.normalized;

        transform.forward = direction;
    }

    private void OnEnable()
    {
        Invoke(nameof(Despawn), lifetime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (shooterRoot != null && other.transform.IsChildOf(shooterRoot))
            return;

        if (other.TryGetComponent(out DamageReciever receiver))
            receiver.RecieveDamage(damage, damageType);

        if (hitVfx != null)
            Instantiate(hitVfx, transform.position, Quaternion.identity);

        Despawn();
    }

    private void Despawn()
    {
        CancelInvoke();
        Destroy(gameObject);
    }
}
