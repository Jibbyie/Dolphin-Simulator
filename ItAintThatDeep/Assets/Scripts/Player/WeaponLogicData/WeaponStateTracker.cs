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

    [Header("Shooting UI Timing")]
    [SerializeField] private float shootingFlashDuration = 0.5f;  // I control how long the shooting icon shows

    public static WeaponState CurrentWeaponState { get; private set; } = WeaponState.Idle;
    public static event Action<WeaponState> OnWeaponStateChanged;

    private FirstPersonShooter shooter;            // I query reload and ammo counts from here
    private WeaponState lastState = WeaponState.Idle;
    private bool isShowingShootingState = false;   // I block other updates while showing shooting
    private Coroutine shootingCoroutine;

    private void Start()
    {
        // I listen for actual fire events and weapon switches
        FirstPersonShooter.OnWeaponFired += OnActualFire;
        WeaponManager.OnWeaponSwitched += OnWeaponSwitch;

        shooter = FindFirstObjectByType<FirstPersonShooter>();
        if (shooter == null)
            Debug.LogError("WeaponStateTracker: Could not find FirstPersonShooter!");

        CameraSwitcher.OnFirstPersonToggled += OnFirstPersonToggle;
    }

    private void Update()
    {
        // I default to Idle when out of first-person or no weapon equipped
        if (!CameraSwitcher.IsFirstPersonActive || WeaponManager.CurrentWeapon == null)
        {
            SetWeaponState(WeaponState.Idle);
            return;
        }

        // I let the shooting flash coroutine drive the state when active
        if (isShowingShootingState)
            return;

        // Otherwise I determine the correct state: Reloading, OutOfAmmo or Idle
        SetWeaponState(DetermineState());
    }

    // I check reload/ammo/input to pick the UI state
    private WeaponState DetermineState()
    {
        var weapon = WeaponManager.CurrentWeapon;
        var type = weapon.weaponType;
        bool currentlyReloading = shooter.GetIsReloading();
        int magazine = shooter.GetMagazineAmmo();
        int reserve = shooter.GetReserveAmmo();

        // Reloading has top priority
        if (FirstPersonShooter.IsRangedWeapon(type) && currentlyReloading)
            return WeaponState.Reloading;

        // Out of ammo when both magazine and reserve are empty
        if (FirstPersonShooter.IsRangedWeapon(type)
            && magazine <= 0
            && reserve <= 0)
            return WeaponState.OutOfAmmo;

        // Shooting input shows a quick flash
        if (IsAttackInput(type))
            return WeaponState.Shooting;

        // Fall back to Idle
        return WeaponState.Idle;
    }

    private bool IsAttackInput(WeaponData.WeaponType type)
    {
        // I mirror the input logic: hold for rifles, click otherwise
        return type == WeaponData.WeaponType.Rifle
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);
    }

    private void OnActualFire()
    {
        // I cancel any existing flash and start a new one
        if (shootingCoroutine != null)
            StopCoroutine(shootingCoroutine);

        StartShootingFlash();
    }

    // I show the Shooting icon then revert after a delay
    private void StartShootingFlash()
    {
        isShowingShootingState = true;
        SetWeaponState(WeaponState.Shooting);

        float duration = WeaponManager.CurrentWeapon.weaponType == WeaponData.WeaponType.Rifle
            ? 0.1f
            : shootingFlashDuration;

        shootingCoroutine = StartCoroutine(ShootingFlashRoutine(duration));
    }

    private IEnumerator ShootingFlashRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isShowingShootingState = false;
        shootingCoroutine = null;

        // If still holding rifle fire, stay Shooting
        if (WeaponManager.CurrentWeapon.weaponType == WeaponData.WeaponType.Rifle
            && Input.GetMouseButton(0))
        {
            SetWeaponState(WeaponState.Shooting);
        }
        else
        {
            SetWeaponState(DetermineState());
        }
    }

    private void OnWeaponSwitch(WeaponData _)
    {
        // I cancel any flash on weapon switch and go Idle
        if (shootingCoroutine != null)
            StopCoroutine(shootingCoroutine);

        isShowingShootingState = false;
        SetWeaponState(WeaponState.Idle);
    }

    private void OnFirstPersonToggle(bool isFP)
    {
        if (!isFP)
        {
            // On exit from first-person, cancel flash and Idle
            if (shootingCoroutine != null)
            {
                StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }
            isShowingShootingState = false;
            SetWeaponState(WeaponState.Idle);
        }
        else
        {
            // On re-entry, re-evaluate immediately
            SetWeaponState(DetermineState());
        }
    }

    // I centralize the OnWeaponFired and switch cleanup
    private void OnDestroy()
    {
        FirstPersonShooter.OnWeaponFired -= OnActualFire;
        WeaponManager.OnWeaponSwitched -= OnWeaponSwitch;
        CameraSwitcher.OnFirstPersonToggled -= OnFirstPersonToggle;

        if (shootingCoroutine != null)
            StopCoroutine(shootingCoroutine);

        OnWeaponStateChanged = null;
    }

    // I change state only when it differs to avoid redundant events
    private void SetWeaponState(WeaponState newState)
    {
        if (newState == lastState) return;
        lastState = newState;
        CurrentWeaponState = newState;
        OnWeaponStateChanged?.Invoke(newState);
    }

}
