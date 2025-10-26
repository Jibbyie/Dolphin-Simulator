using UnityEngine;

/*
Faces the enemy sprite toward the camera's yaw (XZ only, Doom-style).
- Caches the camera once.
- Uses the camera's forward on XZ to avoid wobble when circling.
*/
public class EnemyBillboard : MonoBehaviour
{
    [SerializeField] private Transform target; // camera transform; auto-finds if left empty

    private void Awake()
    {
        // Cache the camera once; avoid per-frame Camera.main lookups
        if (target == null)
        {
            if (Camera.main != null)
            {
                target = Camera.main.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // Use the camera's forward (yaw only) for a stable billboard
        Vector3 faceDir = target.forward;
        faceDir.y = 0f;

        // If forward is nearly zero, bail out
        if (faceDir.sqrMagnitude < 0.0001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(faceDir.normalized, Vector3.up);
    }
}
