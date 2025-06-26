// FirstPersonUIController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FirstPersonUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI weaponNameLabel;        // I display the current weapon's name here
    [SerializeField] private TextMeshProUGUI magazineCountLabel;     // I show bullets left in magazine here
    [SerializeField] private TextMeshProUGUI reserveCountLabel;      // I show bullets left in reserve here
    [SerializeField] private Image weaponIconImage;         // I show the weapon icon

    [Header("Optional Layout Panel")]
    [SerializeField] private GameObject weaponInfoContainer;         // I toggle this panel on/off with camera switch

    private FirstPersonShooter shooter;                               // I pull ammo data from this component

    private void Awake()
    {
        // I cache reference to the shooter for ammo info
        shooter = FindFirstObjectByType<FirstPersonShooter>();
        if (shooter == null)
            Debug.LogError("FirstPersonUIController: Shooter component not found!");

        UpdateFullWeaponDisplay();
    }

    private void OnEnable()
    {
        // I listen for weapon switches and camera toggles
        WeaponManager.OnWeaponSwitched += OnWeaponSwitched;
        CameraSwitcher.OnFirstPersonToggled += OnCameraToggled;

        UpdateFullWeaponDisplay();
    }

    private void OnDisable()
    {
        // I clean up my event subscriptions
        WeaponManager.OnWeaponSwitched -= OnWeaponSwitched;
        CameraSwitcher.OnFirstPersonToggled -= OnCameraToggled;
    }

    private void Update()
    {
        // I update ammo counts each frame when in first-person and a weapon is equipped
        if (CameraSwitcher.IsFirstPersonActive && WeaponManager.CurrentWeapon != null)
        {
            RefreshAmmoDisplay();
        }
    }

    // I update all UI fields: name and ammo
    private void UpdateFullWeaponDisplay()
    {
        var weapon = WeaponManager.CurrentWeapon;
        if (weapon != null)
        {
            DisplayWeaponName(weapon.weaponName);
            DisplayWeaponIcon(weapon.weaponIcon);
            RefreshAmmoDisplay();
        }
        else
        {
            ClearDisplay();
        }
    }

    // I set the weapon name text
    private void DisplayWeaponName(string name)
    {
        if (weaponNameLabel != null)
            weaponNameLabel.text = name;
    }

    private void DisplayWeaponIcon(Sprite icon)
    {
        if(weaponIconImage == null) return;

        if (icon != null)
        {
            weaponIconImage.sprite = icon;
            weaponIconImage.enabled = true; // I show the icon if available
        }
        else
        {
            weaponIconImage.enabled = false; // I hide the icon if not set
        }
    }

    // I update magazine and reserve ammo labels depending on weapon type
    private void RefreshAmmoDisplay()
    {
        if (shooter == null) return;
        var weapon = WeaponManager.CurrentWeapon;
        bool isRanged = weapon.weaponType == WeaponData.WeaponType.Pistol
                      || weapon.weaponType == WeaponData.WeaponType.Rifle
                      || weapon.weaponType == WeaponData.WeaponType.RPG;

        if (isRanged)
        {
            int magCount = shooter.GetMagazineAmmo();
            int reserveCount = shooter.GetReserveAmmo();
            UpdateMagazineLabel(magCount, weapon.magazineSize);
            UpdateReserveLabel(reserveCount);
        }
        else
        {
            // I show "Infinite" for melee weapons
            SetLabelAsInfinite(magazineCountLabel);
            SetLabelAsInfinite(reserveCountLabel);
        }
    }

    // I format magazine label and apply color based on level
    private void UpdateMagazineLabel(int current, int capacity)
    {
        if (magazineCountLabel == null) return;

        if (capacity > 0)
        {
            magazineCountLabel.text = $"{current}/{capacity}";
            magazineCountLabel.color = current == 0 ? Color.red
                                       : current <= capacity * 0.3f ? Color.yellow
                                       : Color.white;
        }
        else
        {
            SetLabelAsInfinite(magazineCountLabel);
        }
    }

    // I format reserve label and apply color based on level
    private void UpdateReserveLabel(int reserve)
    {
        if (reserveCountLabel == null) return;

        if (reserve >= 0)
        {
            reserveCountLabel.text = reserve.ToString();
            reserveCountLabel.color = reserve == 0 ? Color.red
                                      : reserve <= 10 ? Color.yellow
                                      : Color.white;
        }
        else
        {
            SetLabelAsInfinite(reserveCountLabel);
        }
    }

    // I handle panel visibility when camera toggles
    private void OnCameraToggled(bool isFirstPerson)
    {
        if (weaponInfoContainer != null)
            weaponInfoContainer.SetActive(isFirstPerson);
        else
            gameObject.SetActive(isFirstPerson);

        if (isFirstPerson)
            UpdateFullWeaponDisplay();
    }

    // I refresh name and ammo when weapon changes
    private void OnWeaponSwitched(WeaponData newWeapon)
    {
        UpdateFullWeaponDisplay();
    }

    // I clear all UI to default empty state
    private void ClearDisplay()
    {
        if (weaponNameLabel != null)
            weaponNameLabel.text = "No Weapon";
        if (magazineCountLabel != null)
            magazineCountLabel.text = "";
        if (reserveCountLabel != null)
            reserveCountLabel.text = "";
        if (weaponIconImage != null)
            weaponIconImage.enabled = false;
    }

    // I set a text label to show infinity with default color
    private void SetLabelAsInfinite(TextMeshProUGUI label)
    {
        label.text = "Infinite";
        label.color = Color.white;
    }
}
