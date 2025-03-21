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
        if (other.CompareTag("Enemy")) 
        {
            HealthSystem enemyHealth = other.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            Destroy(gameObject); // Destroy projectile on impact
        }
    }
}
