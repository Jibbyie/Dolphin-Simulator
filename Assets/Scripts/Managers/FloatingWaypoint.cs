using UnityEngine;
using TMPro;

public class WaypointLabel : MonoBehaviour
{
    void Update()
    {
        if (Camera.main == null) return;

        Vector3 camDir = transform.position - Camera.main.transform.position;
        transform.rotation = Quaternion.LookRotation(camDir);
    }
}

