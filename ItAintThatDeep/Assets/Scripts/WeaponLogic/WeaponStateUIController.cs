using UnityEngine;
using System;
using System.Collections.Generic;

public class WeaponStateUIController : MonoBehaviour
{
    [Serializable]
    private struct WeaponStateIcons
    {
        [Header("Icons for different states")]
        public WeaponData weaponData;    // I map this weapon’s data to its icons
        public GameObject idleIcon;      // Idle state icon
        public GameObject activeIcon;    // Shooting/attack state icon
        public GameObject reloadingIcon; // Reloading state icon
        public GameObject outOfAmmoIcon; // Out-of-ammo state icon
    }

    [Header("Configure icons for each weapon type below")]
    [SerializeField] private List<WeaponStateIcons> weaponStateIconsList; // I collect inspector configurations

    // I look up icon sets quickly by WeaponData
    private Dictionary<WeaponData, WeaponStateIcons> stateIconsLookup;

    // I track which weapon is currently shown in the UI
    private WeaponData displayedWeapon;

    private void Awake()
    {
        // I build my lookup table for fast icon access
        BuildStateIconsLookup();
        // I set UI active/inactive based on current camera and weapon
        SyncUIWithCurrentWeapon();
    }

    private void OnEnable()
    {
        // I listen for weapon switches, state changes, and camera toggles
        WeaponManager.OnWeaponSwitched += OnWeaponSwitched;
        WeaponStateTracker.OnWeaponStateChanged += OnWeaponStateChanged;
        CameraSwitcher.OnFirstPersonToggled += OnCameraToggled;

        SyncUIWithCurrentWeapon();
    }

    private void OnDisable()
    {
        // I remove all event listeners to avoid memory leaks
        WeaponManager.OnWeaponSwitched -= OnWeaponSwitched;
        WeaponStateTracker.OnWeaponStateChanged -= OnWeaponStateChanged;
        CameraSwitcher.OnFirstPersonToggled -= OnCameraToggled;
    }

    // I translate the inspector list into a dictionary for O(1) lookups
    private void BuildStateIconsLookup()
    {
        stateIconsLookup = new Dictionary<WeaponData, WeaponStateIcons>();
        foreach (var config in weaponStateIconsList)
        {
            if (config.weaponData != null)
                stateIconsLookup[config.weaponData] = config;
        }
    }

    // I update displayedWeapon and refresh icons accordingly
    private void OnWeaponSwitched(WeaponData newWeapon)
    {
        displayedWeapon = newWeapon;
        RefreshIconsForCurrentState();
    }

    // I respond to UI state changes from WeaponStateTracker
    private void OnWeaponStateChanged(WeaponStateTracker.WeaponState state)
    {
        RefreshIconsForCurrentState();
    }

    // I toggle the entire UI panel when entering/exiting first-person
    private void OnCameraToggled(bool isFirstPerson)
    {
        gameObject.SetActive(isFirstPerson);
        if (isFirstPerson)
            RefreshIconsForCurrentState();
    }

    // I sync UI on startup or re-enable
    private void SyncUIWithCurrentWeapon()
    {
        displayedWeapon = WeaponManager.CurrentWeapon;
        RefreshIconsForCurrentState();
    }

    // I hide all icons then show only the relevant one
    private void RefreshIconsForCurrentState()
    {
        // I hide every icon in my lookup
        foreach (var config in stateIconsLookup.Values)
        {
            ToggleIcon(config.idleIcon, false);
            ToggleIcon(config.activeIcon, false);
            ToggleIcon(config.reloadingIcon, false);
            ToggleIcon(config.outOfAmmoIcon, false);
        }

        // I show the correct icon for the current weapon & state
        if (displayedWeapon != null && stateIconsLookup.TryGetValue(displayedWeapon, out var icons))
        {
            ShowIconForState(icons, WeaponStateTracker.CurrentWeaponState);
        }
    }

    // I choose which icon to show based on tracker state
    private void ShowIconForState(WeaponStateIcons icons, WeaponStateTracker.WeaponState state)
    {
        switch (state)
        {
            case WeaponStateTracker.WeaponState.Idle:
                ToggleIcon(icons.idleIcon, true);
                break;

            case WeaponStateTracker.WeaponState.Shooting:
                ToggleIcon(icons.activeIcon, true);
                break;

            case WeaponStateTracker.WeaponState.Reloading:
                // Melee weapons don’t reload visually; show idle instead
                if (IsMeleeWeapon(displayedWeapon.weaponType))
                    ToggleIcon(icons.idleIcon, true);
                else
                    ToggleIcon(icons.reloadingIcon, true);
                break;

            case WeaponStateTracker.WeaponState.OutOfAmmo:
                // Melee weapons never run out; show idle instead
                if (IsMeleeWeapon(displayedWeapon.weaponType))
                    ToggleIcon(icons.idleIcon, true);
                else
                    ToggleIcon(icons.outOfAmmoIcon, true);
                break;
        }
    }

    // I safely enable/disable a single icon GameObject
    private void ToggleIcon(GameObject icon, bool isVisible)
    {
        if (icon != null)
            icon.SetActive(isVisible);
    }

    // I define melee types so I can special-case reload/out-of-ammo
    private bool IsMeleeWeapon(WeaponData.WeaponType type)
    {
        return type == WeaponData.WeaponType.Melee || type == WeaponData.WeaponType.Slap;
    }
}
