using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FirstPersonShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera fpCamera;
    [SerializeField] private AudioSource audioSource;

    [Header("Runtime Weapon State")]
    [SerializeField] private int currentMagazineCount;
    [SerializeField] private int currentReserveMagazineCount;
    [SerializeField] private float nextAllowedFireTime = 0f;
    [SerializeField] private bool isReloading = false;

    // Struct & lookup to remember ammo per weapon
    private struct AmmoState { public int Magazine; public int Reserve; }
    private readonly Dictionary<WeaponData, AmmoState> ammoStateLookup = new Dictionary<WeaponData, AmmoState>();

    private void Start()
    {
        WeaponManager.OnWeaponSwitched += HandleWeaponSwitched;
        if (WeaponManager.CurrentWeapon != null)
        {
            HandleWeaponSwitched(WeaponManager.CurrentWeapon);
        }
    }

    private void OnDestroy()
    {
        WeaponManager.OnWeaponSwitched -= HandleWeaponSwitched;
    }

    private void Update()
    {
        if (!CameraSwitcher.IsFirstPersonActive) return;
        if (WeaponManager.CurrentWeapon == null) return;
        ProcessInput();
    }

    private void ProcessInput()
    {
        WeaponData currentWeaponData = WeaponManager.CurrentWeapon;

        // Check for reload input
        if (Input.GetKeyDown(KeyCode.R) && IsWeaponRanged(currentWeaponData.weaponType))
        {
            AttemptReload();
            return;
        }

        // Determine fire input based on weapon type
        bool isFirePressed = (currentWeaponData.weaponType == WeaponData.WeaponType.Rifle)
            ? Input.GetMouseButton(0)    // hold to fire automatic rifles
            : Input.GetMouseButtonDown(0); // single-fire weapons

        if (isFirePressed)
        {
            AttemptFire(currentWeaponData);
        }
    }

    private void AttemptFire(WeaponData currentWeaponData)
    {
        // Check cooldown and reload status
        if (Time.time < nextAllowedFireTime || isReloading) return;

        bool isRanged = IsWeaponRanged(currentWeaponData.weaponType);
        bool isMelee = (currentWeaponData.weaponType == WeaponData.WeaponType.Melee ||
                        currentWeaponData.weaponType == WeaponData.WeaponType.Slap);

        // Ranged weapons need ammo
        if (isRanged)
        {
            if (currentMagazineCount <= 0)
            {
                AttemptReload(); // auto-reload when magazine empty
                return;
            }

            // Consume one bullet and update stored state
            currentMagazineCount--;
            AmmoState storedState = ammoStateLookup[currentWeaponData];
            storedState.Magazine = currentMagazineCount;
            ammoStateLookup[currentWeaponData] = storedState;
        }

        // Perform the shot
        FireWeapon(currentWeaponData);
    }

    private void FireWeapon(WeaponData currentWeaponData)
    {
        nextAllowedFireTime = Time.time + (1f / currentWeaponData.fireRate);

        if (currentWeaponData.shootSFX != null)
        {
            audioSource.PlayOneShot(currentWeaponData.shootSFX);
        }

        PerformDamageRaycast(currentWeaponData);
    }

    private void AttemptReload()
    {
        // Cannot reload if already reloading or no reserve magazines
        if (isReloading || currentReserveMagazineCount <= 0) return;

        WeaponData currentWeaponData = WeaponManager.CurrentWeapon;
        if (currentMagazineCount >= currentWeaponData.magazineSize) return;

        StartCoroutine(ReloadCoroutine(currentWeaponData));
    }

    private IEnumerator ReloadCoroutine(WeaponData currentWeaponData)
    {
        isReloading = true;
        yield return new WaitForSeconds(currentWeaponData.reloadTime);

        int bulletsNeeded = currentWeaponData.magazineSize - currentMagazineCount;
        int bulletsToReload = Mathf.Min(bulletsNeeded, currentReserveMagazineCount);

        currentMagazineCount += bulletsToReload;
        currentReserveMagazineCount -= bulletsToReload;

        // Persist updated ammo counts
        AmmoState storedState = ammoStateLookup[currentWeaponData];
        storedState.Magazine = currentMagazineCount;
        storedState.Reserve = currentReserveMagazineCount;
        ammoStateLookup[currentWeaponData] = storedState;

        isReloading = false;
    }

    private void PerformDamageRaycast(WeaponData currentWeaponData)
    {
        Ray ray = new Ray(fpCamera.transform.position, fpCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, currentWeaponData.range))
        {
            if (hitInfo.collider.TryGetComponent<DamageReciever>(out var damageReceiver))
            {
                damageReceiver.RecieveDamage(currentWeaponData.damage, currentWeaponData.damageType);
            }
        }
    }

    private bool IsWeaponRanged(WeaponData.WeaponType type)
    {
        return type == WeaponData.WeaponType.Pistol ||
               type == WeaponData.WeaponType.Rifle ||
               type == WeaponData.WeaponType.RPG;
    }

    private void HandleWeaponSwitched(WeaponData newWeapon)
    {
        StopAllCoroutines();
        isReloading = false;

        // Seed or restore ammo state
        if (IsWeaponRanged(newWeapon.weaponType))
        {
            if (!ammoStateLookup.TryGetValue(newWeapon, out AmmoState storedState))
            {
                storedState = new AmmoState
                {
                    Magazine = newWeapon.magazineSize,
                    Reserve = newWeapon.clipSize
                };
                ammoStateLookup[newWeapon] = storedState;
            }

            currentMagazineCount = storedState.Magazine;
            currentReserveMagazineCount = storedState.Reserve;
        }
        else
        {
            // Melee/Slap have infinite uses
            currentMagazineCount = 0;
            currentReserveMagazineCount = 0;
        }

        nextAllowedFireTime = 0f;
    }

    // Expose for UI
    public int GetCurrentMagazineCount() => currentMagazineCount;
    public int GetCurrentReserveMagazineCount() => currentReserveMagazineCount;
    public bool GetIsReloading() => isReloading;
}
