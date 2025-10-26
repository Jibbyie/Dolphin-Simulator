using UnityEngine;

// Attach to enemy prefab. On this enemy’s death, it notifies the KillChainManager once.
[RequireComponent(typeof(DamageReciever))]
public class EnemyKillNotifier : MonoBehaviour
{
    private DamageReciever dr;
    private bool sent;

    private void Awake()
    {
        dr = GetComponent<DamageReciever>();
    }

    private void OnEnable()
    {
        if (dr != null) dr.onDeath.AddListener(OnDeath);
    }

    private void OnDisable()
    {
        if (dr != null) dr.onDeath.RemoveListener(OnDeath);
    }

    private void OnDeath()
    {
        if (sent) return; // just in case
        sent = true;

        var mgr = FindFirstObjectByType<KillChainManager>();
        if (mgr != null) mgr.NotifyKill();
    }
}
