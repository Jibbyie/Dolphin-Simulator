using UnityEngine;
using System;
using System.Collections.Generic;

public class WeaponStateUIController : MonoBehaviour
{
    [Serializable]
    private struct WeaponStateIcons
    {
        [Header("Icons for different weapon states")]
        public WeaponData weaponData;
        public GameObject idleIcon;
        public GameObject activeIcon;
        public GameObject reloadingIcon;
        public GameObject outOfAmmoIcon;
    }

    [Header("Weapon State Icon Configuration")]
    [SerializeField] private List<WeaponStateIcons> weaponStateIconsList;

    private Dictionary<WeaponData, WeaponStateIcons> weaponStateIconsLookup;
    private WeaponData currentDisplayedWeapon;

    private void Awake()
    {
        InitializeWeaponIconsLookup();
        SynchronizeUIWithCurrentWeapon();
    }

    private void OnEnable()
    {
        WeaponManager.OnWeaponSwitched += HandleWeaponSwitched;
        WeaponStateTracker.OnWeaponStateChanged += HandleWeaponStateChanged;
        CameraSwitcher.OnFirstPersonToggled += HandleFirstPersonToggled;

        SynchronizeUIWithCurrentWeapon();
    }

    private void OnDisable()
    {
        WeaponManager.OnWeaponSwitched -= HandleWeaponSwitched;
        WeaponStateTracker.OnWeaponStateChanged -= HandleWeaponStateChanged;
        CameraSwitcher.OnFirstPersonToggled -= HandleFirstPersonToggled;
    }

    private void InitializeWeaponIconsLookup()
    {
        weaponStateIconsLookup = new Dictionary<WeaponData, WeaponStateIcons>();

        foreach (var weaponStateIcons in weaponStateIconsList)
        {
            if (weaponStateIcons.weaponData != null)
            {
                weaponStateIconsLookup[weaponStateIcons.weaponData] = weaponStateIcons;
            }
        }
    }

    private void HandleWeaponSwitched(WeaponData newWeapon)
    {
        currentDisplayedWeapon = newWeapon;
        RefreshWeaponStateDisplay();
    }

    private void HandleWeaponStateChanged(WeaponStateTracker.WeaponState newState)
    {
        RefreshWeaponStateDisplay();
    }

    private void HandleFirstPersonToggled(bool isFirstPersonActive)
    {
        gameObject.SetActive(isFirstPersonActive);
        if (isFirstPersonActive)
        {
            RefreshWeaponStateDisplay();
        }
    }

    private void SynchronizeUIWithCurrentWeapon()
    {
        currentDisplayedWeapon = WeaponManager.CurrentWeapon;
        RefreshWeaponStateDisplay();
    }

    private void RefreshWeaponStateDisplay()
    {
        // Hide all weapon icons first
        HideAllWeaponIcons();

        // Show the appropriate icon for the current weapon and state
        if (currentDisplayedWeapon != null &&
            weaponStateIconsLookup.TryGetValue(currentDisplayedWeapon, out WeaponStateIcons currentWeaponIcons))
        {
            ShowAppropriateWeaponStateIcon(currentWeaponIcons, WeaponStateTracker.CurrentWeaponState);
        }
    }

    private void HideAllWeaponIcons()
    {
        foreach (var weaponIcons in weaponStateIconsLookup.Values)
        {
            SetIconVisibility(weaponIcons.idleIcon, false);
            SetIconVisibility(weaponIcons.activeIcon, false);
            SetIconVisibility(weaponIcons.reloadingIcon, false);
            SetIconVisibility(weaponIcons.outOfAmmoIcon, false);
        }
    }

    private void ShowAppropriateWeaponStateIcon(WeaponStateIcons weaponIcons, WeaponStateTracker.WeaponState currentState)
    {
        switch (currentState)
        {
            case WeaponStateTracker.WeaponState.Idle:
                SetIconVisibility(weaponIcons.idleIcon, true);
                break;

            case WeaponStateTracker.WeaponState.Shooting:
                SetIconVisibility(weaponIcons.activeIcon, true);
                break;

            case WeaponStateTracker.WeaponState.Reloading:
                // For melee weapons, show idle instead of reloading
                if (IsMeleeWeapon(currentDisplayedWeapon.weaponType))
                {
                    SetIconVisibility(weaponIcons.idleIcon, true);
                }
                else
                {
                    SetIconVisibility(weaponIcons.reloadingIcon, true);
                }
                break;

            case WeaponStateTracker.WeaponState.OutOfAmmo:
                // For melee weapons, show idle instead of out of ammo
                if (IsMeleeWeapon(currentDisplayedWeapon.weaponType))
                {
                    SetIconVisibility(weaponIcons.idleIcon, true);
                }
                else
                {
                    SetIconVisibility(weaponIcons.outOfAmmoIcon, true);
                }
                break;
        }
    }

    private void SetIconVisibility(GameObject iconGameObject, bool isVisible)
    {
        if (iconGameObject != null)
        {
            iconGameObject.SetActive(isVisible);
        }
    }

    private bool IsMeleeWeapon(WeaponData.WeaponType weaponType)
    {
        return weaponType == WeaponData.WeaponType.Melee ||
               weaponType == WeaponData.WeaponType.Slap;
    }
}