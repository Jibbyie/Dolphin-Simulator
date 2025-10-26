using UnityEngine;
using TMPro;

// Tracks kill streaks within a combo window.
// Shows a small top-right label when a tier is reached and plays a one-shot SFX.
public class KillChainManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI tierLabel; // Top-right label

    [Header("Combo Window")]
    [SerializeField] private float comboWindowSeconds = 4f;

    [Header("Tiers")]
    [SerializeField] private int[] tierThresholds = new int[] { 3, 5, 8, 12 };
    [SerializeField] private string[] tierNames = new string[] { "RAMPAGE", "UNSTOPPABLE", "MAYHEM", "GODLIKE" };
    [SerializeField] private AudioClip[] tierSfx; // optional, same length as thresholds (or shorter)

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; // optional; any AudioSource to play SFX

    private int streakCount;
    private float comboTimer;
    private int tierIndex; // -1 means "not at a tier"

    private void Awake()
    {
        HideLabel();
        ResetChain();
    }

    private void Update()
    {
        if (streakCount <= 0) return;

        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0f)
        {
            // Chain ended
            ResetChain();
            HideLabel();
        }
    }

    // Called by EnemyKillNotifier on enemy death
    public void NotifyKill()
    {
        streakCount++;
        comboTimer = comboWindowSeconds;

        // Check if we crossed a new tier
        int newTier = GetTierIndexFor(streakCount);
        if (newTier > tierIndex)
        {
            tierIndex = newTier;
            ShowTier(tierIndex);
            PlayTierSfx(tierIndex);
        }
    }

    private int GetTierIndexFor(int count)
    {
        int idx = -1;
        for (int i = 0; i < tierThresholds.Length; i++)
        {
            if (count >= tierThresholds[i]) idx = i;
        }
        return idx;
    }

    private void ShowTier(int index)
    {
        if (tierLabel == null) return;
        if (index < 0 || index >= tierNames.Length) return;

        tierLabel.text = tierNames[index];
        if (!tierLabel.gameObject.activeSelf) tierLabel.gameObject.SetActive(true);
    }

    private void HideLabel()
    {
        if (tierLabel != null) tierLabel.gameObject.SetActive(false);
    }

    private void PlayTierSfx(int index)
    {
        if (audioSource == null || tierSfx == null) return;
        if (index < 0 || index >= tierSfx.Length) return;
        var clip = tierSfx[index];
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    private void ResetChain()
    {
        streakCount = 0;
        comboTimer = 0f;
        tierIndex = -1;
    }
}
