// EnemyShooter.cs
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    [Tooltip("Empty GameObject or bone from which projectiles spawn")]
    [SerializeField] private Transform shootPoint;
    [Tooltip("Prefab must have EnemyProjectile component")]
    [SerializeField] private GameObject projectilePrefab;
    [Tooltip("Shots per second")]
    [SerializeField] private float fireRate = 1f;

    private float _nextFireTime;
    private Transform _player;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;
    }

    void Update()
    {
        if (_player == null) return;

        if (Time.time >= _nextFireTime)
        {
            ShootAt(_player.position);
            _nextFireTime = Time.time + 1f / fireRate;
        }
    }

    private void ShootAt(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - shootPoint.position).normalized;
        var projGO = Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(dir));
        if (projGO.TryGetComponent<EnemyProjectile>(out var proj))
        {
            proj.Initialize(dir);
        }
    }
}
