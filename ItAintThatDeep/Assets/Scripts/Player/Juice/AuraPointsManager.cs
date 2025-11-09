using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AuraPointsManager : MonoBehaviour
{
    // UI listeners subscribe here (always has a valid world position)
    public static Action<int, Vector3> OnPointsAdded;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI pointsLabel;

    [Header("Scoring")]
    [SerializeField] private int basePointsPerKill = 2;
    [SerializeField] private List<int> tierMultipliers = new List<int> { 2, 3, 2 };

    private int totalPoints;

    private void OnEnable()
    {
        KillChainManager.OnKill += HandleKillWithPos;
        UpdateLabel();
    }

    private void OnDisable()
    {
        KillChainManager.OnKill -= HandleKillWithPos;
    }

    private void HandleKillWithPos(Vector3 worldPos)
    {
        var mgr = FindFirstObjectByType<KillChainManager>();
        int tierIdx = (mgr != null) ? mgr.CurrentTierIndex : -1;

        int mult = 1;
        if (tierIdx >= 0 && tierMultipliers != null && tierMultipliers.Count > 0)
        {
            int end = Mathf.Min(tierIdx + 1, tierMultipliers.Count);
            for (int i = 0; i < end; i++)
                mult *= Mathf.Max(1, tierMultipliers[i]);
        }

        int added = basePointsPerKill * mult;
        totalPoints += added;
        UpdateLabel();

        OnPointsAdded?.Invoke(added, worldPos);
    }

    private void UpdateLabel()
    {
        if (pointsLabel == null)
            return;

        // Displays points first, then the aura icon
        pointsLabel.text =
            $"{totalPoints}<space=0.25em><size=185%><voffset=0.12em><sprite name=Aura></voffset></size>";
    }

    public static void AddBonusPoints(int amount, Vector3 worldPos, string label = null)
    {
        var mgr = FindFirstObjectByType<AuraPointsManager>();
        if (mgr == null) return;

        // Update total
        mgr.totalPoints += Mathf.Max(0, amount);
        mgr.UpdateLabel();

        // Tell floaters to show the bonus. If you want a custom label, prefix it.
        int shown = Mathf.Max(0, amount);
        if (!string.IsNullOrEmpty(label))
        {
            // Show "TRICKSHOT +50" (or your label) via one combined string:
            AuraFloaterUI.SpawnStatic(worldPos, label + " +" + shown);
        }
        else
        {
            // Fall back to normal points-added event if you prefer:
            if (OnPointsAdded != null) OnPointsAdded.Invoke(shown, worldPos);
        }
    }

}
