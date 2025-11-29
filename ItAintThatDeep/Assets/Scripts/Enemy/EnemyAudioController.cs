using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(DamageReciever))]
public class EnemyAudioController : MonoBehaviour
{
    [Header("Hit Sounds")]
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private float hitVolume = 1f;
    [SerializeField] private Vector2 hitPitchRange = new Vector2(0.9f, 1.1f);

    [Header("Attack Sounds")]
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private float attackVolume = 1f;
    [SerializeField] private Vector2 attackPitchRange = new Vector2(0.9f, 1.1f);

    [Header("Death Sounds")]
    [SerializeField] private AudioClip[] deathSounds;
    [SerializeField] private float deathVolume = 1f;
    [SerializeField] private Vector2 deathPitchRange = new Vector2(0.9f, 1.1f);

    [Header("Prefabs")]
    [SerializeField] private GameObject deathAudioPrefab;

    private AudioSource audioSource;
    private DamageReciever damageReciever;
    private bool deathPlayed = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 30f;

        damageReciever = GetComponent<DamageReciever>();
    }

    private void OnEnable()
    {
        damageReciever.onHit.AddListener(PlayHitSound);
        damageReciever.onDeath.AddListener(PlayDeathSound);
    }

    private void OnDisable()
    {
        damageReciever.onHit.RemoveListener(PlayHitSound);
        damageReciever.onDeath.RemoveListener(PlayDeathSound);
    }

    private void PlayHitSound(float dmg, WeaponData.DamageType type)
    {
        if (hitSounds == null || hitSounds.Length == 0) return;

        AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];

        audioSource.Stop();
        audioSource.pitch = Random.Range(hitPitchRange.x, hitPitchRange.y) * SloMo.CurrentPitchScale;
        audioSource.PlayOneShot(clip, hitVolume);
    }

    public void PlayAttackSound()
    {
        if (attackSounds == null || attackSounds.Length == 0) return;

        AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];

        audioSource.Stop();
        audioSource.pitch = Random.Range(attackPitchRange.x, attackPitchRange.y) * SloMo.CurrentPitchScale;
        audioSource.PlayOneShot(clip, attackVolume);
    }

    private void PlayDeathSound()
    {
        if (deathPlayed) return;
        deathPlayed = true;

        if (deathSounds == null || deathSounds.Length == 0) return;

        AudioClip clip = deathSounds[Random.Range(0, deathSounds.Length)];

        if (deathAudioPrefab != null)
        {
            GameObject obj = Instantiate(deathAudioPrefab, transform.position, Quaternion.identity);

            AudioSource src = obj.GetComponent<AudioSource>();
            src.pitch = Random.Range(deathPitchRange.x, deathPitchRange.y) * SloMo.CurrentPitchScale;
            src.PlayOneShot(clip, deathVolume);

            Destroy(obj, clip.length + 0.5f);
        }
        else
        {
            audioSource.Stop();
            audioSource.pitch = Random.Range(deathPitchRange.x, deathPitchRange.y) * SloMo.CurrentPitchScale;
            audioSource.PlayOneShot(clip, deathVolume);
        }
    }
}
