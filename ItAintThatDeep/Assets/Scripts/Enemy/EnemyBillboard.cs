using UnityEngine;

/*
Faces the enemy sprite toward the camera's yaw (XZ only, Doom-style).
Also spawns a minimap marker prefab (CanvasMark) that hovers above the enemy
and matches the same facing direction as the sprite.
*/
[DisallowMultipleComponent]
public class EnemyBillboard : MonoBehaviour
{
    [Header("Billboard")]
    [SerializeField] private Transform target; // camera transform; auto-finds if left empty

    [Header("Minimap Marker")]
    [Tooltip("Prefab for the minimap marker (CanvasMark). Different enemies can use different prefabs).")]
    [SerializeField] private GameObject minimapPrefab;

    [Tooltip("Vertical offset of the marker above the enemy.")]
    [SerializeField] private float markerYOffset = 2f;

    private Transform markerInstance;

    private void Awake()
    {
        // Cache camera once; avoids repeated Camera.main lookups.
        if (target == null && Camera.main != null)
        {
            target = Camera.main.transform;
        }

        // Spawn minimap marker if prefab assigned.
        if (minimapPrefab != null)
        {
            GameObject marker = Instantiate(minimapPrefab);
            markerInstance = marker.transform;

            marker.layer = LayerMask.NameToLayer("Minimap");

            marker.name = $"{gameObject.name}_Marker";
        }
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            // --- Billboard rotation for sprite (yaw only) ---
            Vector3 faceDir = target.forward;
            faceDir.y = 0f;

            if (faceDir.sqrMagnitude > 0.0001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(faceDir.normalized, Vector3.up);
                transform.rotation = lookRot;

                // --- Apply same facing to marker so it matches sprite orientation ---
                if (markerInstance != null)
                    markerInstance.rotation = lookRot;
            }
        }

        // --- Update marker position (hover above enemy) ---
        if (markerInstance != null)
        {
            Vector3 pos = transform.position;
            pos.y += markerYOffset;
            markerInstance.position = pos;
        }
    }

    private void OnDisable()
    {
        if (markerInstance != null)
        {
            Destroy(markerInstance.gameObject);
            markerInstance = null;
        }
    }

    private void OnDestroy()
    {
        if (markerInstance != null)
        {
            Destroy(markerInstance.gameObject);
            markerInstance = null;
        }
    }
}
