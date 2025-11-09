using UnityEngine;

// Listens to KillChainManager.OnKill(worldPos) and grants a bonus if the spin qualifies.
public class TrickshotManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TrickshotSpinTracker spinTracker;

    [Header("Reward")]
    [SerializeField] private int bonusPoints = 50;
    [SerializeField] private string floaterText = "TRICKSHOT\u00A0+\u00A050";

    [Header("SFX (optional)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip trickshotSfx;
    [SerializeField] private float sfxVolume = 1f;

    private void Awake()
    {
        if (spinTracker == null)
            spinTracker = FindFirstObjectByType<TrickshotSpinTracker>();
    }

    private void OnEnable()
    {
        KillChainManager.OnKill += OnKillAtWorldPos;
    }

    private void OnDisable()
    {
        KillChainManager.OnKill -= OnKillAtWorldPos;
    }

    private void OnKillAtWorldPos(Vector3 worldPos)
    {
        if (spinTracker != null && spinTracker.ConsumeIfReady())
        {
            // Add to total and pop a "+bonus" at the kill point
            AuraPointsManager.AddBonusPoints(bonusPoints, worldPos, floaterText);

            if (sfxSource != null && trickshotSfx != null)
                sfxSource.PlayOneShot(trickshotSfx, sfxVolume);
        }
        else
        {
            // If not qualified, clear any stale spin when slo-mo ends naturally
            if (!SloMo.IsActive && spinTracker != null)
                spinTracker.ForceReset();
        }
    }
}
