using System.Collections;
using UnityEngine;

/*
Spawns one or more death FX prefabs when this enemy dies.

• Listens to DamageReciever.onDeath and triggers exactly once.
• Instantiates all configured deathPrefabs at the enemy's position/rotation.
• Estimates a safe lifetime from child components (Animator, AudioSource, ParticleSystem).
• Destroys each spawned FX object after the estimated lifetime.
• Falls back to a fixed lifetime if nothing provides a duration.
*/
[RequireComponent(typeof(DamageReciever))]
public class EnemyDeathJuice : MonoBehaviour
{
    [Header("Death Prefabs (shared)")]
    [SerializeField] private GameObject[] deathPrefabs;

    [Header("Lifetime Fallback (sec)")]
    [SerializeField] private float fallbackLifetime = 0.4f;

    private DamageReciever damageReciever;
    private bool handled;

    private void Awake()
    {
        damageReciever = GetComponent<DamageReciever>();
    }

    private void OnEnable()
    {
        if (damageReciever != null)
        {
            damageReciever.onDeath.AddListener(OnDeath);
        }
    }

    private void OnDisable()
    {
        if (damageReciever != null)
        {
            damageReciever.onDeath.RemoveListener(OnDeath);
        }
    }

    /*
    Public helper in case other systems want to trigger this manually.
    This will no-op if already handled.
    */
    public void Trigger()
    {
        OnDeath();
    }

    /*
    Death event handler. Spawns FX once and schedules cleanup.
    */
    private void OnDeath()
    {
        if (handled == true)
        {
            return;
        }

        handled = true;

        if (deathPrefabs == null)
        {
            return;
        }

        int count = deathPrefabs.Length;
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = deathPrefabs[i];
            if (prefab == null)
            {
                continue;
            }

            GameObject fxRoot = Instantiate(prefab, transform.position, transform.rotation);

            float life = EstimateLifetime(fxRoot);
            if (life <= 0f)
            {
                life = fallbackLifetime;
            }

            Destroy(fxRoot, life);
        }
    }

    /*
    Attempts to compute a safe lifetime for an FX root by inspecting common components.
    Uses the maximum value it finds across all relevant children.
    */
    private float EstimateLifetime(GameObject fxRoot)
    {
        float life = 0f;

        // Animator clips: choose the longest clip on the controller
        Animator animator = fxRoot.GetComponentInChildren<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            var clips = animator.runtimeAnimatorController.animationClips;
            if (clips != null)
            {
                int clipCount = clips.Length;
                for (int i = 0; i < clipCount; i++)
                {
                    AnimationClip clip = clips[i];
                    if (clip != null)
                    {
                        if (clip.length > life)
                        {
                            life = clip.length;
                        }
                    }
                }
            }
        }

        // AudioSources: take the longest clip length on any enabled source
        AudioSource[] audioSources = fxRoot.GetComponentsInChildren<AudioSource>(true);
        if (audioSources != null)
        {
            int audioCount = audioSources.Length;
            for (int i = 0; i < audioCount; i++)
            {
                AudioSource src = audioSources[i];
                if (src != null)
                {
                    if (src.clip != null)
                    {
                        if (src.clip.length > life)
                        {
                            life = src.clip.length;
                        }
                    }
                }
            }
        }

        // ParticleSystems: duration + max start lifetime is a safe upper bound
        ParticleSystem[] systems = fxRoot.GetComponentsInChildren<ParticleSystem>(true);
        if (systems != null)
        {
            int psCount = systems.Length;
            for (int i = 0; i < psCount; i++)
            {
                ParticleSystem ps = systems[i];
                if (ps != null)
                {
                    var main = ps.main;
                    float estimate = main.duration + main.startLifetime.constantMax;
                    if (estimate > life)
                    {
                        life = estimate;
                    }
                }
            }
        }

        // If nothing provided a value, return 0 and let caller apply fallbackLifetime
        return life;
    }
}
