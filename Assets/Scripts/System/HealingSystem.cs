using UnityEngine;

public class HealingSystem : MonoBehaviour
{
    [Header("Healing Settings")]
    public float healingPerSecond = 5f; // Healing over time (e.g., in a danger area)
    public bool isContinuousHealing = true; // If true, healing is applied per second
    public bool isInstantHealing = false; // If true, healing is applied once on enter
    public float instantHealingAmount = 20f;

    private void OnTriggerEnter(Collider other)
    {
        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null)
        {
            if (isInstantHealing)
            {
                health.HealDamage(instantHealingAmount);
                Object.Destroy(gameObject);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null && isContinuousHealing)
        {
            health.HealDamage(healingPerSecond * Time.deltaTime);
        }
    }
}
