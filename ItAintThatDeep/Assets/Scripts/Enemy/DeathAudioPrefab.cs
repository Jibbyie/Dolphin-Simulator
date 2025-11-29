using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SloMoAudio))]
public class DeathAudioPrefab : MonoBehaviour
{
    private void Awake()
    {
        AudioSource src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 1f;
        src.minDistance = 2f;
        src.maxDistance = 30f;
    }
}
