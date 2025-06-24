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
            if(WeaponManager.CurrentWeapon.weaponName == "Slap")
            {
                Slap();
            }
            else
            {
                Shoot();
            }
        }
    }

    void Slap()
    {
        audioSource.PlayOneShot(WeaponManager.CurrentWeapon.shootSFX);

        // short range raycast
        Ray ray = new Ray(fpCamera.transform.position, fpCamera.transform.forward);
        if(Physics.Raycast(ray, out var hit, WeaponManager.CurrentWeapon.range))
        {
            var col = hit.collider;
            bool handled = false;

            // First, try any interaction
            if(col.TryGetComponent<IInteractable>(out var inter))
            {
                inter.Interact();
                handled = true;
            }

            // then, apply minimal damage if possible
            if(col.TryGetComponent<IDamageable>(out var dmg))
            {
                dmg.TakeDamage(WeaponManager.CurrentWeapon.damage);
                handled = true;
            }

            if(!handled)
            {
                Debug.Log($"Slapped {col.name}, but nothing happened.");
            }

        }
    }

    void Shoot()
    {
        audioSource.PlayOneShot(WeaponManager.CurrentWeapon.shootSFX);
        
        Ray ray = new Ray(fpCamera.transform.position, fpCamera.transform.forward);
        if(Physics.Raycast(ray, out RaycastHit hit, WeaponManager.CurrentWeapon.range, hitLayers))
        {
            // Try to apply damage:
            if(hit.collider.TryGetComponent<IDamageable>(out var target))
            {
                bool died = target.TakeDamage(WeaponManager.CurrentWeapon.damage);
                Debug.Log($"Hit {hit.collider.name}, dealt {WeaponManager.CurrentWeapon.damage} dmg. Died? {died}");
            }
            else
            {
                Debug.Log($"Hit {hit.collider.name}, but it's not damageable.");
            }
        }
    }
}
