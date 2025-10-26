using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SprintParticleSystem : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles; // auto-filled if left empty
    private bool wasSprinting;

    private void Reset()
    {
        particles = GetComponent<ParticleSystem>();
    }

    private void Awake()
    {
        if (particles == null) particles = GetComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        wasSprinting = false;
    }

    private void Update()
    {
        bool sprinting = FirstPersonController.IsSprinting; // your sprint flag
        if (sprinting && !wasSprinting)
        {
            particles.Play();
        }
        else if (!sprinting && wasSprinting)
        {
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        wasSprinting = sprinting;
    }
}
