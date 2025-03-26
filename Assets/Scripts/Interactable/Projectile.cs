using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 3f;
    public float damage = 10f;

    private void Start()
    {
        Destroy(gameObject, lifeTime); // Destroy projectile after a few seconds to save memory
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        HealthSystem targetHealth = other.GetComponent<HealthSystem>();
        if (targetHealth != null)
        {
            // Prevent friendly fire by checking tag difference
            bool isPlayerShooting = gameObject.CompareTag("PlayerProjectile");
            bool isEnemyShooting = gameObject.CompareTag("EnemyProjectile");

            if (isPlayerShooting && other.CompareTag("Enemy"))
            {
                targetHealth.TakeDamage(damage);
                Destroy(gameObject);
            }
            else if (isEnemyShooting && other.CompareTag("Player"))
            {
                targetHealth.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }

}
