using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Health_Slap : MonoBehaviour, ISlapable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private UnityEvent onSlapped;
    private float currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public bool TakeDamage(float amount)
    {
        onSlapped?.Invoke();
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
