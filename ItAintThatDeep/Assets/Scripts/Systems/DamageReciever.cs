using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class DamageReciever : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Damage Immunities")]
    [SerializeField]
    public List<WeaponData.DamageType> DamageTypeImmunities = new List<WeaponData.DamageType>();

    [Header("Events")]
    [SerializeField] private UnityEvent<float, WeaponData.DamageType> onHit;
    [SerializeField] private UnityEvent onDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void RecieveDamage(float amount, WeaponData.DamageType damageType)
    {
        if(DamageTypeImmunities.Contains(damageType))
        {
            Debug.Log($"{name} is immune to {damageType}");
            return;
        }

        currentHealth -= amount;
        onHit?.Invoke(amount, damageType);
        Debug.Log($"{name} took {amount} damage of type {damageType}. HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        
            onDeath?.Invoke();
            Debug.Log($"{name} has died.");
            //Destroy(gameObject);
    }
}
