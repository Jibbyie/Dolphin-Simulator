using UnityEngine;

[RequireComponent(typeof(DamageReciever))]
public class EnemyHealthSpriteSwapper : MonoBehaviour
{
    [Header("Sprite Renderer Reference")]
    [SerializeField, Tooltip("I will swap this SpriteRenderer's sprite based on my health. Leave null to auto-find in children.")]
    private SpriteRenderer spriteRenderer;

    [Header("Health Thresholds (fractions of max health)")]
    [SerializeField, Tooltip("When health  this fraction, I show my original sprite.")]
    private float highHealthThreshold = 0.75f;
    [SerializeField, Tooltip("When health  this fraction but  high, I show my mildly hurt sprite.")]
    private float mediumHealthThreshold = 0.40f;

    [Header("Health State Sprites")]
    [SerializeField, Tooltip("I show this sprite when I'm mildly hurt (health between medium and high).")]
    private Sprite mildlyHurtSprite;
    [SerializeField, Tooltip("I show this sprite when I'm very injured (health below medium).")]
    private Sprite veryInjuredSprite;

    // I cache whatever sprite was set at start so I can restore it at high health.
    private Sprite originalSprite;

    // I need the DamageReciever so I know my current vs. max health.
    private DamageReciever damageReceiver;

    private void Awake()
    {
        // If the user didn't assign a SpriteRenderer, I look for one on my children.
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
                Debug.LogError($"{name}: No SpriteRenderer found on me or my children!");
        }

        // I remember the starting sprite as my "full health" appearance.
        originalSprite = spriteRenderer.sprite;

        // I grab the DamageReciever so I can monitor health changes.
        damageReceiver = GetComponent<DamageReciever>();
        if (damageReceiver == null)
            Debug.LogError($"{name}: EnemyHealthSpriteSwapper requires a DamageReciever component.");
    }

    private void Start()
    {
        // As soon as I spawn, I set the correct sprite based on my current health.
        UpdateSpriteBasedOnHealth();
    }

    private void OnEnable()
    {
        // Whenever I take damage, I re-evaluate which sprite I should be using.
        damageReceiver.onHit.AddListener(OnHitEvent);
    }

    private void OnDisable()
    {
        damageReceiver.onHit.RemoveListener(OnHitEvent);
    }

    // Called by DamageReciever each time I take damage.
    private void OnHitEvent(float damageAmount, WeaponData.DamageType damageType)
    {
        // I swap to the correct sprite right after my health changes.
        UpdateSpriteBasedOnHealth();
    }

    // I compute my health fraction and choose:
    private void UpdateSpriteBasedOnHealth()
    {
        float healthFraction = damageReceiver.CurrentHealth / damageReceiver.MaxHealth;

        if (healthFraction >= highHealthThreshold)
        {
            // High health  restore my original appearance.
            spriteRenderer.sprite = originalSprite;
        }
        else if (healthFraction >= mediumHealthThreshold)
        {
            // Medium health  show mildly hurt sprite.
            if (mildlyHurtSprite != null)
                spriteRenderer.sprite = mildlyHurtSprite;
            else
                Debug.LogWarning($"{name}: mildlyHurtSprite not assigned!");
        }
        else
        {
            // Low health  show very injured sprite.
            if (veryInjuredSprite != null)
                spriteRenderer.sprite = veryInjuredSprite;
            else
                Debug.LogWarning($"{name}: veryInjuredSprite not assigned!");
        }
    }
}
