using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(DamageReciever))]
public class PlayerHealthController : MonoBehaviour
{
    [Header("HP Bar Sprites")]
    [SerializeField] private Image hpBarImage;      // The image to update
    [SerializeField] private Sprite[] hpSprites;    // 11 sprites, 0 = empty, 10 = full

    [Header("Hit Overlay")]
    [SerializeField] private Image hitOverlayImage;
    [SerializeField] private float hitOverlayDuration = 0.2f;

    [Header("Hit Audio")]
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioClip[] hitSoundEffects;

    DamageReciever damageReceiver;
    Coroutine hitOverlayCoroutine;

    void Awake()
    {
        damageReceiver = GetComponent<DamageReciever>();
        damageReceiver.onHit.AddListener(HandleHitEvent);
        damageReceiver.onDeath.AddListener(HandleDeathEvent);
        UpdateHpBarSprite();
    }

    void HandleHitEvent(float damageAmount, WeaponData.DamageType damageType)
    {
        UpdateHpBarSprite();

        if (hitAudioSource != null && hitSoundEffects != null && hitSoundEffects.Length > 0)
        {
            int randomIndex = Random.Range(0, hitSoundEffects.Length);
            var clip = hitSoundEffects[randomIndex];
            if (clip != null)
                hitAudioSource.PlayOneShot(clip);
        }

        if (hitOverlayCoroutine != null)
            StopCoroutine(hitOverlayCoroutine);

        hitOverlayCoroutine = StartCoroutine(PlayHitOverlayFlash());
    }

    void UpdateHpBarSprite()
    {
        if (hpSprites == null || hpSprites.Length == 0 || hpBarImage == null)
            return;

        float healthPercent = damageReceiver.CurrentHealth / damageReceiver.MaxHealth;
        int index = Mathf.RoundToInt(healthPercent * (hpSprites.Length - 1));
        index = Mathf.Clamp(index, 0, hpSprites.Length - 1);
        hpBarImage.sprite = hpSprites[index];
    }

    IEnumerator PlayHitOverlayFlash()
    {
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

        hitOverlayImage.gameObject.SetActive(false);
        hitOverlayCoroutine = null;
    }

    void HandleDeathEvent()
    {
        UpdateHpBarSprite();
    }
}
