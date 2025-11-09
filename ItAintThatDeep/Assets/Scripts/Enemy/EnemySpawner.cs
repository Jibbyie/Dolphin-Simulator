using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] GameObject[] enemyPrefabs;   // assign all enemy prefabs here
    [SerializeField] int spawnCount = 10;         // total enemies to spawn
    [SerializeField] float spawnDelay = 1f;       // delay between each spawn
    [SerializeField] Vector2 spawnAreaSize = new Vector2(20f, 20f);
    [SerializeField] float minSpawnDistance = 2f; // spacing between enemies
    [SerializeField] float spawnHeight = 0f;

    [Header("Debug")]
    [SerializeField] bool drawGizmos = true;

    readonly List<Vector3> usedPositions = new();

    void Start()
    {
        StartCoroutine(SpawnEnemiesSequentially());
    }

    IEnumerator SpawnEnemiesSequentially()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("No enemy prefabs assigned to EnemySpawner.");
            yield break;
        }

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = spawnCount * 10;

        while (spawned < spawnCount && attempts < maxAttempts)
        {
            attempts++;

            Vector3 candidate = GetRandomPosition();
            if (!IsPositionValid(candidate))
                continue;

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Instantiate(prefab, candidate, Quaternion.identity);
            usedPositions.Add(candidate);
            spawned++;

            yield return new WaitForSeconds(spawnDelay);
        }

        if (spawned < spawnCount)
            Debug.LogWarning($"Only spawned {spawned}/{spawnCount} enemies (not enough space).");
    }

    Vector3 GetRandomPosition()
    {
        float x = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
        float z = Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f);
        return new Vector3(transform.position.x + x, spawnHeight, transform.position.z + z);
    }

    bool IsPositionValid(Vector3 pos)
    {
        for (int i = 0; i < usedPositions.Count; i++)
        {
            if (Vector3.Distance(pos, usedPositions[i]) < minSpawnDistance)
                return false;
        }
        return true;
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector3(transform.position.x, spawnHeight, transform.position.z),
            new Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.y)
        );
    }
}
