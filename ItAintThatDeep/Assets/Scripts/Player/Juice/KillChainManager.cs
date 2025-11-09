using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KillChainManager : MonoBehaviour
{
    // Single kill event that always includes the world position
    public static Action<Vector3> OnKill;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI tierLabel;

    [Header("Combo Window")]
    [SerializeField] private float comboWindowSeconds = 4f;

    [Header("Tiers (edit in Inspector)")]
    [SerializeField] private List<int> tierThresholds = new List<int> { 3, 6, 10, 15 };
    [SerializeField] private List<string> tierNames = new List<string> { "RAMPAGE", "UNSTOPPABLE", "MAYHEM", "GODLIKE" };
    [SerializeField] private List<AudioClip> tierSfx = new List<AudioClip>();

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Shake (higher tier = stronger)")]
    [SerializeField] private float baseShake = 1.0f;
    [SerializeField] private float shakePerTier = 0.75f;
    [SerializeField] private float maxShake = 6.0f;

    private RectTransform labelRT;
    private Vector2 labelBasePos;
    private bool basePosCaptured;

    private int streakCount;
    private float comboTimer;
    private int tierIndex = -1;

    private void Awake()
    {
        HideLabel();
        ResetChain();

        if (tierLabel != null)
        {
            labelRT = tierLabel.rectTransform;
            labelBasePos = labelRT.anchoredPosition;
            basePosCaptured = true;
        }
    }

    private void Update()
    {
        if (streakCount > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                ResetChain();
                HideLabel();
                ApplyShake(true);
                return;
            }
        }

        bool reset = (tierIndex < 0) || tierLabel == null || !tierLabel.gameObject.activeSelf;
        ApplyShake(reset);
    }

    // NEW: call this from EnemyKillNotifier, passing the enemy's world position
    public void NotifyKillAt(Vector3 worldPos)
    {
        streakCount++;
        comboTimer = comboWindowSeconds;

        int newTier = GetTierIndexFor(streakCount);
        if (newTier > tierIndex)
        {
            tierIndex = newTier;
            ShowTier(tierIndex);
            PlayTierSfx(tierIndex);
        }

        // Fire single, ordered event with position (no race)
        OnKill?.Invoke(worldPos);
    }

    private int GetTierIndexFor(int count)
    {
        if (tierThresholds == null || tierThresholds.Count == 0) return -1;
        int idx = -1;
        for (int i = 0; i < tierThresholds.Count; i++)
            if (count >= tierThresholds[i]) idx = i;
        return idx;
    }

    private void ShowTier(int index)
    {
        if (tierLabel == null) return;

        string name = (tierNames != null && index >= 0 && index < tierNames.Count) ? tierNames[index] : string.Empty;
        if (string.IsNullOrEmpty(name))
        {
            tierLabel.gameObject.SetActive(false);
            return;
        }

        if (!basePosCaptured)
        {
            labelRT = tierLabel.rectTransform;
            labelBasePos = labelRT.anchoredPosition;
            basePosCaptured = true;
        }

        tierLabel.text = name;
        if (!tierLabel.gameObject.activeSelf) tierLabel.gameObject.SetActive(true);
    }

    private void HideLabel()
    {
        if (tierLabel != null) tierLabel.gameObject.SetActive(false);
    }

    private void PlayTierSfx(int index)
    {
        if (audioSource == null || tierSfx == null) return;
        if (index < 0 || index >= tierSfx.Count) return;
        var clip = tierSfx[index];
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    private void ResetChain()
    {
        streakCount = 0;
        comboTimer = 0f;
        tierIndex = -1;
    }

    public int CurrentTierIndex => tierIndex;
    public int CurrentStreakCount => streakCount;
    public int TierCount => tierThresholds != null ? tierThresholds.Count : 0;

    private void ApplyShake(bool resetToBase)
    {
        if (labelRT == null || !basePosCaptured) return;
        if (resetToBase) { labelRT.anchoredPosition = labelBasePos; return; }

        int effectiveTier = Mathf.Max(0, tierIndex);
        float amp = Mathf.Min(baseShake + shakePerTier * effectiveTier, maxShake);
        Vector2 offset = UnityEngine.Random.insideUnitCircle * amp;
        labelRT.anchoredPosition = labelBasePos + offset;
    }
}
