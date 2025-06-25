using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("World units per second")]
    [SerializeField] private float speed = 10f;
    [Tooltip("Damage dealt on hit")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private WeaponData.DamageType damageType;
    [Tooltip("Seconds before self destruct")]
    [SerializeField] private float lifetime = 5f;

    private Vector3 _direction;

    // Call this right after Instantiate
    public void Initialize(Vector3 direction)
    {
        _direction = direction;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += _direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<DamageReciever>(out var dr))
        {
            dr.RecieveDamage(damage, damageType);
        }
        // Always destroy on any hit
        Destroy(gameObject);
    }
}
