using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Tooltip("Maximum enemies alive at once.")]
    [SerializeField] private int maxConcurrentEnemies = 5;

    [Tooltip("Total enemies allowed to be spawned over time.")]
    [SerializeField] private int totalSpawnBudget = 20;

    [SerializeField] private float spawnDelay = 1f;
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(20f, 20f);
    [SerializeField] private float minSpawnDistance = 2f;
    [SerializeField] private float spawnHeight = 0f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    private readonly List<Vector3> usedPositions = new();
    private readonly HashSet<GameObject> activeEnemies = new();

    private int totalSpawned = 0;
    private bool spawning = false;

    private void Start()
    {
        StartCoroutine(SpawnerLoop());
    }

    private IEnumerator SpawnerLoop()
    {
        spawning = true;

        while (totalSpawned < totalSpawnBudget)
        {
            // Wait until we are below the max allowed enemies
            while (activeEnemies.Count >= maxConcurrentEnemies)
            {
                yield return null;
            }

            // Attempt a spawn
            TrySpawnOne();

            yield return new WaitForSeconds(spawnDelay);
        }

        spawning = false;
    }

    private void TrySpawnOne()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("No enemy prefabs assigned to EnemySpawner.");
            return;
        }

        if (totalSpawned >= totalSpawnBudget)
            return;

        // Find a valid position
        const int maxAttempts = 15;
        Vector3 candidate = Vector3.zero;
        bool valid = false;

        for (int i = 0; i < maxAttempts; i++)
        {
            candidate = GetRandomPosition();
            if (IsPositionValid(candidate))
            {
                valid = true;
                break;
            }
        }

        if (!valid)
        {
            Debug.LogWarning("EnemySpawner: Could not find valid spawn position.");
            return;
        }

        // Spawn
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject enemy = Instantiate(prefab, candidate, Quaternion.identity);

        totalSpawned++;
        usedPositions.Add(candidate);
        activeEnemies.Add(enemy);

        // Track when enemy dies
        var dr = enemy.GetComponent<DamageReciever>();
        if (dr != null)
        {
            dr.onDeath.AddListener(() =>
            {
                activeEnemies.Remove(enemy);
            });
        }
        else
        {
            // fallback: auto-remove after destroy
            StartCoroutine(RemoveOnDestroy(enemy));
        }
    }

    private IEnumerator RemoveOnDestroy(GameObject enemy)
    {
        yield return new WaitUntil(() => enemy == null);
        activeEnemies.Remove(enemy);
    }

    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
        float z = Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f);
        return new Vector3(transform.position.x + x, spawnHeight, transform.position.z + z);
    }

    private bool IsPositionValid(Vector3 pos)
    {
        for (int i = 0; i < usedPositions.Count; i++)
        {
            if (Vector3.Distance(pos, usedPositions[i]) < minSpawnDistance)
                return false;
        }
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector3(transform.position.x, spawnHeight, transform.position.z),
            new Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.y)
        );
    }
}
