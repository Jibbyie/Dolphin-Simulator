using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(DamageReciever))]
public class PlayerHealthController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider healthBarSlider;           // I show the player's current health here
    [SerializeField] private Image hitOverlayImage;            // I flash this image when the player takes damage
    [SerializeField] private float hitOverlayDuration = 0.2f;  // I control how long the hit flash lasts

    [Header("Hit Audio")]
    [SerializeField] private AudioSource hitAudioSource;       // I play hit sound effects through this
    [SerializeField] private AudioClip[] hitSoundEffects;      // I pick a random clip from these when hit

    // I receive health events from the DamageReciever component
    private DamageReciever damageReceiver;
    // I keep track of the running overlay coroutine so I can stop it if another hit occurs
    private Coroutine hitOverlayCoroutine;

    private void Awake()
    {
        // I cache the DamageReciever to get health info and subscribe to its events
        damageReceiver = GetComponent<DamageReciever>();

        // I initialize the health bar to the player's starting health
        healthBarSlider.maxValue = damageReceiver.MaxHealth;
        healthBarSlider.value = damageReceiver.CurrentHealth;

        // I subscribe to damage and death callbacks
        damageReceiver.onHit.AddListener(HandleHitEvent);
        damageReceiver.onDeath.AddListener(HandleDeathEvent);
    }

    // I respond to the damage event: update UI, play sound, and flash overlay
    private void HandleHitEvent(float damageAmount, WeaponData.DamageType damageType)
    {
        // I reflect the new health in the UI slider
        healthBarSlider.value = damageReceiver.CurrentHealth;

        // I play a random hit sound if any are assigned
        if (hitAudioSource != null && hitSoundEffects != null && hitSoundEffects.Length > 0)
        {
            int randomIndex = Random.Range(0, hitSoundEffects.Length);
            var clip = hitSoundEffects[randomIndex];
            if (clip != null)
                hitAudioSource.PlayOneShot(clip);
        }

        // I restart the hit overlay fade if we're already flashing
        if (hitOverlayCoroutine != null)
            StopCoroutine(hitOverlayCoroutine);

        // I start the overlay fade coroutine
        hitOverlayCoroutine = StartCoroutine(PlayHitOverlayFlash());
    }

    // I fade the hit overlay from full to transparent over hitOverlayDuration
    private IEnumerator PlayHitOverlayFlash()
    {
        // I enable the overlay and set full opacity
        hitOverlayImage.gameObject.SetActive(true);
        Color overlayColor = hitOverlayImage.color;
        overlayColor.a = 1f;
        hitOverlayImage.color = overlayColor;

        float elapsed = 0f;
        while (elapsed < hitOverlayDuration)
        {
            elapsed += Time.deltaTime;
            var alpha = Mathf.Lerp(1f, 0f, elapsed / hitOverlayDuration);
            overlayColor.a = alpha;
            hitOverlayImage.color = overlayColor;
            yield return null;
        }

        // I hide the overlay once fade completes
        hitOverlayImage.gameObject.SetActive(false);
        hitOverlayCoroutine = null;
    }

    // I respond when the player dies; currently I log a message
    private void HandleDeathEvent()
    {
       
    }
}
