using UnityEngine;

public class RewardPickup : MonoBehaviour
{
    public ParticleSystem sparkleEffect;
    public AudioSource pickupSound;

    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            collected = true;

            if (pickupSound != null)
                pickupSound.Play();

            if (sparkleEffect != null)
                sparkleEffect.Play();

            // Hide mesh
            foreach (var mesh in GetComponentsInChildren<MeshRenderer>())
                mesh.enabled = false;

            // Destroy after sound/particles finish
            Destroy(gameObject, 1.5f);
        }
    }
}
