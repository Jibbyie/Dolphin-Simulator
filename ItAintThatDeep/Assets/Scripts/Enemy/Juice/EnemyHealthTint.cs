using UnityEngine;

/*
Tints the enemy sprite based on remaining health.
- High (>= highThreshold): white
- Medium (>= mediumThreshold and < highThreshold): yellow
- Low (< mediumThreshold): red
*/
[RequireComponent(typeof(DamageReciever))]
public class EnemyHealthTint : MonoBehaviour
{
    [Header("Target Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Thresholds (fractions of max health)")]
    [SerializeField] private float highThreshold = 0.75f;
    [SerializeField] private float mediumThreshold = 0.40f;

    [Header("Tint Colors (defaults: white / yellow / red)")]
    [SerializeField] private Color highTint = Color.white;
    [SerializeField] private Color mediumTint = Color.yellow;
    [SerializeField] private Color lowTint = Color.red;

    private DamageReciever damageReceiver;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        damageReceiver = GetComponent<DamageReciever>();
        ValidateThresholds();
    }

    // EnemyHealthTint.cs
    private void OnEnable()
    {
        if (damageReceiver != null)
        {
            damageReceiver.onHit.AddListener(OnHit);
            damageReceiver.onDeath.AddListener(OnDeath);
        }

        // Delay first tint one frame so DamageReciever has init’d
        StartCoroutine(ApplyNextFrame());
    }

    private System.Collections.IEnumerator ApplyNextFrame()
    {
        yield return null; // wait one frame
        ApplyHealthTint();
    }

    private void OnDisable()
    {
        if (damageReceiver != null)
        {
            damageReceiver.onHit.RemoveListener(OnHit);
            damageReceiver.onDeath.RemoveListener(OnDeath);
        }
    }

    // Public manual refresh
    public void ApplyHealthTint()
    {
        float fraction = GetHealthFractionSafe();
        SetTintForFraction(fraction);
    }

    private void OnHit(float damage, WeaponData.DamageType type)
    {
        ApplyHealthTint();
    }

    private void OnDeath()
    {
        ApplyHealthTint();
    }

    // Chooses a tint color based on current health fraction
    private void SetTintForFraction(float fraction)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Color target;

        if (fraction >= highThreshold)
        {
            target = highTint;
        }
        else
        {
            if (fraction >= mediumThreshold)
            {
                target = mediumTint;
            }
            else
            {
                target = lowTint;
            }
        }

        spriteRenderer.color = target;
    }

    // Clamp thresholds to [0,1] and keep high >= medium
    private void ValidateThresholds()
    {
        if (highThreshold < 0f) highThreshold = 0f;
        if (highThreshold > 1f) highThreshold = 1f;

        if (mediumThreshold < 0f) mediumThreshold = 0f;
        if (mediumThreshold > 1f) mediumThreshold = 1f;

        if (mediumThreshold > highThreshold)
        {
            float t = mediumThreshold;
            mediumThreshold = highThreshold;
            highThreshold = t;
        }
    }

    private float GetHealthFractionSafe()
    {
        if (damageReceiver == null)
        {
            return 1f;
        }

        float value = damageReceiver.GetHealthFraction();
        if (value < 0f) value = 0f;
        if (value > 1f) value = 1f;
        return value;
    }
}
