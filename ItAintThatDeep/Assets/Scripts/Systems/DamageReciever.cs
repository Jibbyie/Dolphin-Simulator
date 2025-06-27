using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageReciever : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField]
    private float maxHealth = 100f;                          // I define the maximum health this entity can have

    private float currentHealth;                             // I track the entity’s current health points

    // I expose current and maximum health for other systems
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    [Header("Damage Immunities")]
    [Tooltip("I ignore damage types listed here")]
    [SerializeField]
    public List<WeaponData.DamageType> DamageTypeImmunities = new List<WeaponData.DamageType>();

    [Header("Events")]
    // I fire onHit when damage is applied, and onDeath when health drops to zero
    public UnityEvent<float, WeaponData.DamageType> onHit;
    public UnityEvent onDeath;

    private void Awake()
    {
        // I initialize current health to the maximum at startup
        currentHealth = maxHealth;
    }

    public void RecieveDamage(float amount, WeaponData.DamageType damageType)
    {
        if (!enabled) return;
        // I skip damage if this damage type is listed in my immunities
        if (DamageTypeImmunities.Contains(damageType))
        {
            Debug.Log($"{name} is immune to {damageType} damage.");
            return;
        }

        // I subtract the damage from my current health
        currentHealth -= amount;

        // I notify listeners that I was hit
        onHit?.Invoke(amount, damageType);
        Debug.Log($"{name} took {amount} damage of type {damageType}. Current HP: {currentHealth}");

        // I check if I've run out of health and should die
        if (currentHealth <= 0f)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        onDeath?.Invoke();
        Debug.Log($"{name} has died.");
    }
}
