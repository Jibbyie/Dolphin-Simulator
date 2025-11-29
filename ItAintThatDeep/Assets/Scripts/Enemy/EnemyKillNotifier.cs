using UnityEngine;

[RequireComponent(typeof(DamageReciever))]
public class EnemyKillNotifier : MonoBehaviour
{
    private DamageReciever dr;
    private bool sent;

    [Header("Ammo Drop Settings")]
    [SerializeField] private GameObject ammoDropPrefab;
    [SerializeField][Range(0f, 1f)] private float dropChance = 0.3f;


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


    private void TrySpawnAmmoDrop()
    {
        if (ammoDropPrefab == null) return;
        if (Random.value > dropChance) return;

        Instantiate(ammoDropPrefab, transform.position, Quaternion.identity);
    }

    private void OnDeath()
    {
        TrySpawnAmmoDrop();
        Debug.Log($"Drop roll: {Random.value}, chance {dropChance}", this);

        if (sent) return;
        sent = true;

        var mgr = FindFirstObjectByType<KillChainManager>();
        if (mgr != null)
        {
            mgr.NotifyKillAt(transform.position);
        }

        var player = FindFirstObjectByType<PlayerHealthController>();
        if (player != null)
        {
            float hp = player.GetComponent<DamageReciever>().CurrentHealth;
            float heal = 0f;

            if (hp < 30f) heal = 30f;
            else if (hp < 60f) heal = 15f;
            else if (hp < 80f) heal = 10f;

            if (heal > 0f)
                player.Heal(heal);
        }
    }
}
