using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public bool TakeDamage(float amount)
    {
        currentHealth -= amount;
        if(currentHealth <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
