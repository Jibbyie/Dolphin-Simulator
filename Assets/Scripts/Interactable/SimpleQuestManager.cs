using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleQuestManager : MonoBehaviour
{
    public static SimpleQuestManager Instance;

    public bool missionAccepted = false;
    public bool enemyDefeated = false;
    public bool rewardClaimed = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
}
