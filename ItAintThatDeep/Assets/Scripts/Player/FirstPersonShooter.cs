using UnityEngine;

public class FirstPersonShooter : MonoBehaviour
{
    [SerializeField] private Camera fpCamera;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSFX;

    void Update()
    {
        if (!CameraSwitcher.IsFirstPersonActive) return;

        if (Input.GetMouseButtonDown(0)) // Left click to shoot
        {
            Shoot();
        }
    }

    void Shoot()
    {
        audioSource.PlayOneShot(shootSFX);

        Ray ray = new Ray(fpCamera.transform.position, fpCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitLayers))
        {
            Debug.Log("Hit: " + hit.collider.name);
            Destroy(hit.collider.gameObject);
        }
    }
}
