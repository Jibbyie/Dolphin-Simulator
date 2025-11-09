using UnityEngine;

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
        if (sent) return;
        sent = true;

        var mgr = FindFirstObjectByType<KillChainManager>();
        if (mgr != null)
        {
            // Pass the enemy's world position directly
            mgr.NotifyKillAt(transform.position);
        }
    }
}
