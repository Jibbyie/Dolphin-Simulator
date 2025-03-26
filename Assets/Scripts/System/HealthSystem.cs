using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI (Optional)")]
    public Text healthText; // For dolphin HUD
    public TextMeshPro floatingHealthText; 

    [Header("Audio (Optional)")]
    public AudioSource deathSound;

    private AudioSource[] allAudioSources;
    private BreathingSystem breathingSystem;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        breathingSystem = GetComponent<BreathingSystem>();
        allAudioSources = GetComponents<AudioSource>();
        UpdateHealthText();
    }

    void Update()
    {
        if (!isDead)
        {
            if (CompareTag("Player") && breathingSystem != null && breathingSystem.GetCurrentBreath() <= 0)
            {
                TakeDamage(maxHealth);
            }

            UpdateHealthText();

            if (floatingHealthText != null)
            {
                // Make floating text always face the camera
                Vector3 camDir = floatingHealthText.transform.position - Camera.main.transform.position;
                floatingHealthText.transform.rotation = Quaternion.LookRotation(camDir);

            }

            if (IsDead())
            {
                Die();
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthText();

        if (IsDead())
        {
            Die();
        }
    }

    public void HealDamage(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthText();
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + Mathf.CeilToInt(currentHealth) + "/" + maxHealth;
        }

        if (floatingHealthText != null)
        {
            floatingHealthText.text = Mathf.CeilToInt(currentHealth).ToString();
        }
    }

    void Die()
    {
        isDead = true;

        if (CompareTag("Player"))
        {
            Debug.Log("Player has died!");
            StopAllSounds();

            if (deathSound != null)
                deathSound.Play();

            Destroy(gameObject, deathSound != null ? deathSound.clip.length : 0f);
        }
        else if (CompareTag("Enemy"))
        {
            Debug.Log("Enemy died!");
            SimpleQuestManager.Instance.enemyDefeated = true;

            if (SimpleQuestManager.Instance.questGiver != null)
            {
                SimpleQuestManager.Instance.questGiver.OnEnemyDefeated();
            }

            Destroy(gameObject);
        }
    }

    void StopAllSounds()
    {
        foreach (AudioSource audio in allAudioSources)
        {
            audio.Stop();
        }
    }
}
