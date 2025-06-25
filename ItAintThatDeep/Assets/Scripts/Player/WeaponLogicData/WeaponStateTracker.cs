using UnityEngine;
using System;
using System.Collections;

public class WeaponStateTracker : MonoBehaviour
{
    public enum WeaponState
    {
        Idle,
        Shooting,
        Reloading,
        OutOfAmmo
    }

    [Header("Shooting Display Settings")]
    [SerializeField] private float shootingStateDisplayDuration = 0.5f;

    public static WeaponState CurrentWeaponState { get; private set; } = WeaponState.Idle;
    public static event Action<WeaponState> OnWeaponStateChanged;

    private FirstPersonShooter shooterReference;
    private WeaponState previousState = WeaponState.Idle;
    private bool isInShootingDisplayMode = false;
    private Coroutine shootingDisplayCoroutine;

    private void Start()
    {
        // Find your shooter and subscribe to camera toggles
        shooterReference = FindFirstObjectByType<FirstPersonShooter>();
        if (shooterReference == null)
            Debug.LogError("WeaponStateTracker: Could not find FirstPersonShooter component!");

        CameraSwitcher.OnFirstPersonToggled += HandleFirstPersonToggled;
    }

    private void Update()
    {
        // If not in first-person or no weapon, force Idle
        if (!CameraSwitcher.IsFirstPersonActive || WeaponManager.CurrentWeapon == null)
        {
            UpdateWeaponState(WeaponState.Idle);
            return;
        }

        // Otherwise, decide whether to show Shooting (via coroutine) or the true state
        WeaponState newState = DetermineCurrentWeaponState();

        if (newState == WeaponState.Shooting && !isInShootingDisplayMode)
        {
            StartShootingDisplay();
        }
        else if (newState != WeaponState.Shooting && !isInShootingDisplayMode)
        {
            UpdateWeaponState(newState);
        }
        // While in shooting display mode, the coroutine will handle the transition back
    }

    private void HandleFirstPersonToggled(bool isFirstPersonActive)
    {
        if (!isFirstPersonActive)
        {
            // Exiting first-person: cancel any Shooting display and reset to Idle
            if (shootingDisplayCoroutine != null)
            {
                StopCoroutine(shootingDisplayCoroutine);
                shootingDisplayCoroutine = null;
            }
            isInShootingDisplayMode = false;
            UpdateWeaponState(WeaponState.Idle);
        }
        else
        {
            // Entering first-person: immediately re-evaluate actual state
            UpdateWeaponState(DetermineCurrentWeaponState());
        }
    }

    private WeaponState DetermineCurrentWeaponState()
    {
        var currentWeapon = WeaponManager.CurrentWeapon;

        // Reloading has top priority for ranged
        if (IsWeaponRanged(currentWeapon.weaponType) && shooterReference.GetIsReloading())
            return WeaponState.Reloading;

        // Out of ammo
        if (IsWeaponRanged(currentWeapon.weaponType) &&
            shooterReference.GetCurrentMagazineCount() <= 0 &&
            shooterReference.GetCurrentReserveMagazineCount() <= 0)
            return WeaponState.OutOfAmmo;

        // Shooting/attacking
        if (IsCurrentlyAttacking(currentWeapon.weaponType))
            return WeaponState.Shooting;

        return WeaponState.Idle;
    }

    private bool IsCurrentlyAttacking(WeaponData.WeaponType weaponType)
    {
        if (weaponType == WeaponData.WeaponType.Rifle)
            return Input.GetMouseButton(0);

        return Input.GetMouseButtonDown(0);
    }

    private void StartShootingDisplay()
    {
        if (shootingDisplayCoroutine != null)
            StopCoroutine(shootingDisplayCoroutine);

        var duration = GetShootingDisplayDuration(WeaponManager.CurrentWeapon.weaponType);
        shootingDisplayCoroutine = StartCoroutine(ShootingDisplayCoroutine(duration));
    }

    private float GetShootingDisplayDuration(WeaponData.WeaponType weaponType)
    {
        return weaponType == WeaponData.WeaponType.Rifle
            ? 0.1f
            : shootingStateDisplayDuration;
    }

    private IEnumerator ShootingDisplayCoroutine(float displayDuration)
    {
        isInShootingDisplayMode = true;
        UpdateWeaponState(WeaponState.Shooting);

        yield return new WaitForSeconds(displayDuration);

        isInShootingDisplayMode = false;

        var postState = DetermineCurrentWeaponState();
        if (postState == WeaponState.Shooting)
        {
            // For rifles, if still holding, stay Shooting
            if (WeaponManager.CurrentWeapon.weaponType == WeaponData.WeaponType.Rifle &&
                Input.GetMouseButton(0))
            {
                UpdateWeaponState(WeaponState.Shooting);
            }
            else
            {
                UpdateWeaponState(WeaponState.Idle);
            }
        }
        else
        {
            UpdateWeaponState(postState);
        }

        shootingDisplayCoroutine = null;
    }

    private bool IsWeaponRanged(WeaponData.WeaponType weaponType)
    {
        return weaponType == WeaponData.WeaponType.Pistol ||
               weaponType == WeaponData.WeaponType.Rifle ||
               weaponType == WeaponData.WeaponType.RPG;
    }

    private void UpdateWeaponState(WeaponState newState)
    {
        if (newState == previousState) return;
        previousState = newState;
        CurrentWeaponState = newState;
        OnWeaponStateChanged?.Invoke(newState);
    }

    private void OnDestroy()
    {
        // Clean up coroutine and event subscriptions
        if (shootingDisplayCoroutine != null)
            StopCoroutine(shootingDisplayCoroutine);

        CameraSwitcher.OnFirstPersonToggled -= HandleFirstPersonToggled;
        OnWeaponStateChanged = null;
    }
}
