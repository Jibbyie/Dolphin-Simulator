using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WeaponData;

public class FirstPersonShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera firstPersonCamera;       // I store the player’s camera reference
    [SerializeField] private AudioSource weaponAudioSource;   // I play firing and reload sounds here

    [Header("Layer Masking")]
    [Tooltip("Layers that can be hit by weapons")]
    [SerializeField] private LayerMask hittableLayers;        // I use this to filter raycasts/spherecasts

    [Header("Current Ammo & Timing")]
    [SerializeField] private int magazineAmmoCount;           // I track bullets in the current magazine
    [SerializeField] private int reserveAmmoCount;            // I track bullets in reserve
    [SerializeField] private float nextFireTimestamp = 0f;    // I enforce the fire rate cooldown
    [SerializeField] private bool isReloading = false;        // I prevent firing while reloading

    // I remember ammo counts per weapon to restore when switching
    private struct AmmoState { public int magazine; public int reserve; }
    private readonly Dictionary<WeaponData, AmmoState> ammoStatesByWeapon = new();

    // I signal to UI that a shot has successfully fired
    public static event Action OnWeaponFired;

    private void Start()
    {
        // I listen for weapon switches to seed or restore ammo
        WeaponManager.OnWeaponSwitched += HandleWeaponSwitched;
        if (WeaponManager.CurrentWeapon != null)
            HandleWeaponSwitched(WeaponManager.CurrentWeapon);
    }

    private void OnDestroy()
    {
        WeaponManager.OnWeaponSwitched -= HandleWeaponSwitched;
    }

    private void Update()
    {
        // I only process input when in first-person and a weapon is equipped
        if (!CameraSwitcher.IsFirstPersonActive || WeaponManager.CurrentWeapon == null)
            return;

        ProcessPlayerInput();
    }

    // I check for reload or fire inputs each frame
    private void ProcessPlayerInput()
    {
        var weaponData = WeaponManager.CurrentWeapon;

        // Reload key pressed
        if (Input.GetKeyDown(KeyCode.R) && IsRangedWeapon(weaponData.weaponType))
        {
            AttemptReload();
            return;
        }

        // Determine firing input based on weapon type
        bool fireInput = weaponData.weaponType == WeaponData.WeaponType.Rifle
            ? Input.GetMouseButton(0)         // hold for automatic fire
            : Input.GetMouseButtonDown(0);    // click for single shots

        if (fireInput)
            AttemptFire(weaponData);
    }

    // I handle firing logic: cooldown and ammo checks
    private void AttemptFire(WeaponData weaponData)
    {
        // Respect fire rate and reload state
        if (Time.time < nextFireTimestamp || isReloading)
            return;

        if (IsRangedWeapon(weaponData.weaponType))
        {
            // Auto-reload if out of bullets
            if (magazineAmmoCount <= 0)
            {
                AttemptReload();
                return;
            }

            // I consume one bullet from the magazine
            magazineAmmoCount--;
            var savedState = ammoStatesByWeapon[weaponData];
            savedState.magazine = magazineAmmoCount;
            ammoStatesByWeapon[weaponData] = savedState;
        }

        FireWeapon(weaponData);
    }

    // I perform the shot: cooldown timestamp, audio, damage, and auto-reload
    private void FireWeapon(WeaponData weaponData)
    {
        nextFireTimestamp = Time.time + 1f / weaponData.fireRate;
        OnWeaponFired?.Invoke();

        if (weaponData.shootSFX != null)
            weaponAudioSource.PlayOneShot(weaponData.shootSFX);

        PerformDamageRaycast(weaponData);

        // Auto-reload when magazine is empty but reserve has ammo
        if (IsRangedWeapon(weaponData.weaponType)
            && magazineAmmoCount <= 0
            && reserveAmmoCount > 0)
        {
            AttemptReload();
        }
    }

    // I start the reload coroutine if possible
    private void AttemptReload()
    {
        if (isReloading || reserveAmmoCount <= 0)
            return;

        var weaponData = WeaponManager.CurrentWeapon;
        if (magazineAmmoCount >= weaponData.magazineSize)
            return;

        StartCoroutine(ReloadRoutine(weaponData));
    }

    // I wait for reloadTime, then transfer ammo from reserve to magazine
    private IEnumerator ReloadRoutine(WeaponData weaponData)
    {
        isReloading = true;
        if (weaponData.reloadSFX != null)
            weaponAudioSource.PlayOneShot(weaponData.reloadSFX);

        yield return new WaitForSeconds(weaponData.reloadTime);

        int needed = weaponData.magazineSize - magazineAmmoCount;
        int toLoad = Mathf.Min(needed, reserveAmmoCount);

        magazineAmmoCount += toLoad;
        reserveAmmoCount -= toLoad;

        var savedState = ammoStatesByWeapon[weaponData];
        savedState.magazine = magazineAmmoCount;
        savedState.reserve = reserveAmmoCount;
        ammoStatesByWeapon[weaponData] = savedState;

        isReloading = false;
    }

    // I handle applying damage based on weapon type (single target or splash) and respect enemy immunities
    private void PerformDamageRaycast(WeaponData weaponData)
    {
        // I define the ray origin and direction from the player camera
        Vector3 rayOrigin = firstPersonCamera.transform.position;
        Vector3 rayDirection = firstPersonCamera.transform.forward;
        Ray attackRay = new Ray(rayOrigin, rayDirection);

        float damageAmount = weaponData.damage;
        DamageType damageType = weaponData.damageType;

        // I only apply damage and trigger hit reactions if the target is not immune
        void ApplyDamageIfVulnerable(DamageReciever receiver, RaycastHit hitInfo)
        {
            // I skip both damage and flashing if this damageType is listed as immune
            if (receiver.DamageTypeImmunities.Contains(damageType))
                return;

            // I apply the damage to the receiver
            receiver.RecieveDamage(damageAmount, damageType);

            // I notify any hit reactable component to trigger flash or other VFX
            if (hitInfo.collider.TryGetComponent<IHitReactable>(out var reaction))
                reaction.OnHit(hitInfo);
        }

        if (weaponData.weaponType == WeaponData.WeaponType.RPG)
        {
            // I deal splash damage via a sphere cast
            RaycastHit[] splashHits = Physics.SphereCastAll(
                attackRay,
                weaponData.sphereCastRadius,
                weaponData.range,
                hittableLayers
            );

            foreach (var hitInfo in splashHits)
            {
                // I ignore myself
                if (hitInfo.collider.gameObject == gameObject)
                    continue;

                // I apply damage only if the object has a DamageReciever
                if (hitInfo.collider.TryGetComponent<DamageReciever>(out var receiver))
                    ApplyDamageIfVulnerable(receiver, hitInfo);
            }
        }
        else
        {
            // I deal direct damage via a single raycast
            if (Physics.Raycast(attackRay, out var hitInfo, weaponData.range, hittableLayers)
                && hitInfo.collider.TryGetComponent<DamageReciever>(out var receiver))
            {
                ApplyDamageIfVulnerable(receiver, hitInfo);
            }
        }
    }

    // I reset ammo and state when the player switches weapons
    private void HandleWeaponSwitched(WeaponData newWeapon)
    {
        StopAllCoroutines();
        isReloading = false;

        if (IsRangedWeapon(newWeapon.weaponType))
        {
            if (!ammoStatesByWeapon.TryGetValue(newWeapon, out AmmoState savedState))
            {
                savedState = new AmmoState
                {
                    magazine = newWeapon.magazineSize,
                    reserve = newWeapon.clipSize
                };
                ammoStatesByWeapon[newWeapon] = savedState;
            }
            magazineAmmoCount = savedState.magazine;
            reserveAmmoCount = savedState.reserve;
        }
        else
        {
            magazineAmmoCount = 0;
            reserveAmmoCount = 0;
        }

        nextFireTimestamp = 0f;
    }

    // I define what counts as a ranged weapon in one central place
    public static bool IsRangedWeapon(WeaponData.WeaponType type) =>
        type == WeaponData.WeaponType.Pistol ||
        type == WeaponData.WeaponType.Rifle ||
        type == WeaponData.WeaponType.RPG;

    // I expose ammo and reload state for UI
    public int GetMagazineAmmo() => magazineAmmoCount;
    public int GetReserveAmmo() => reserveAmmoCount;
    public bool GetIsReloading() => isReloading;
}
