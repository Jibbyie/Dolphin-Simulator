using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damagePerSecond = 5f; // Damage over time (e.g., in a danger area)
    public bool isContinuousDamage = true; // If true, damage is applied per second
    public bool isInstantDamage = false; // If true, damage is applied once on enter
    public float instantDamageAmount = 20f;

    private void OnTriggerEnter(Collider other)
    {
        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null)
        {
            if (isInstantDamage)
            {
                health.TakeDamage(instantDamageAmount);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null && isContinuousDamage)
        {
            health.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}
