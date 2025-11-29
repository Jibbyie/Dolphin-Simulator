using UnityEngine;
using System.Collections.Generic;

public class FishSpawner : MonoBehaviour
{
    [Header("Fish Prefabs (multiple allowed)")]
    [SerializeField] private GameObject[] fishPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private int totalFishToSpawn = 50;
    [SerializeField] private float spawnRadius = 80f;
    [SerializeField] private float minHeight = -10f;
    [SerializeField] private float maxHeight = 100f;
    [SerializeField] private float minFishSpacing = 8f;

    [Header("Collision Check")]
    [SerializeField] private float safeCheckRadius = 1.5f;
    [SerializeField] private LayerMask collisionMask;

    private List<Vector3> usedPositions = new List<Vector3>();

    private void Awake()
    {
        SpawnFish();
        Destroy(gameObject);
    }

    private void SpawnFish()
    {
        if (fishPrefabs == null || fishPrefabs.Length == 0)
        {
            Debug.LogError("FishSpawner: No fish prefabs assigned.");
            return;
        }

        int attempts = 0;

        for (int i = 0; i < totalFishToSpawn; i++)
        {
            Vector3 position;

            while (true)
            {
                attempts++;
                if (attempts > totalFishToSpawn * 50)
                {
                    Debug.LogWarning("FishSpawner: Could not place all fish safely.");
                    return;
                }

                position = GenerateRandomPosition();

                if (!Physics.CheckSphere(position, safeCheckRadius, collisionMask) &&
                    IsFarFromOthers(position))
                {
                    usedPositions.Add(position);
                    break;
                }
            }

            GameObject prefab = fishPrefabs[Random.Range(0, fishPrefabs.Length)];
            Instantiate(prefab, position, Quaternion.identity);
        }
    }

    // ------------------------------
    // Position Generation
    // ------------------------------

    private Vector3 GenerateRandomPosition()
    {
        Vector2 circle = Random.insideUnitCircle * spawnRadius;

        float y = Random.Range(minHeight, maxHeight);

        return new Vector3(
            transform.position.x + circle.x,
            y,
            transform.position.z + circle.y
        );
    }

    // ------------------------------
    // Validation Checks
    // ------------------------------

    private bool IsFarFromOthers(Vector3 pos)
    {
        foreach (var p in usedPositions)
        {
            if (Vector3.Distance(pos, p) < minFishSpacing)
                return false;
        }
        return true;
    }

    // ------------------------------
    // Gizmos
    // ------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.3f, 1f, 0.15f);
        Gizmos.DrawSphere(transform.position, spawnRadius);
    }
}
