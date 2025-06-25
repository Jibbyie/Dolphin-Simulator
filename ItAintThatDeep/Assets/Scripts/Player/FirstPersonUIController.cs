using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FirstPersonUIController : MonoBehaviour
{
    [Header("Weapon Info Display")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI currentMagazineText;
    [SerializeField] private TextMeshProUGUI currentClipText;

    [Header("UI Layout (Optional)")]
    [SerializeField] private GameObject weaponInfoPanel;

    private FirstPersonShooter shooterReference;

    private void Awake()
    {
        shooterReference = FindFirstObjectByType<FirstPersonShooter>();
        if (shooterReference == null)
        {
            Debug.LogError("FirstPersonUIController: Could not find FirstPersonShooter component!");
        }

        RefreshWeaponInfoDisplay();
    }

    private void OnEnable()
    {
        WeaponManager.OnWeaponSwitched += HandleWeaponSwitched;
        CameraSwitcher.OnFirstPersonToggled += HandleFirstPersonToggled;

        RefreshWeaponInfoDisplay();
    }

    private void OnDisable()
    {
        WeaponManager.OnWeaponSwitched -= HandleWeaponSwitched;
        CameraSwitcher.OnFirstPersonToggled -= HandleFirstPersonToggled;
    }

    private void Update()
    {
        // Update ammo display continuously for real-time feedback
        if (CameraSwitcher.IsFirstPersonActive && WeaponManager.CurrentWeapon != null)
        {
            UpdateAmmoDisplay();
        }
    }

    private void HandleWeaponSwitched(WeaponData newWeapon)
    {
        RefreshWeaponInfoDisplay();
    }

    private void HandleFirstPersonToggled(bool isFirstPersonActive)
    {
        if (weaponInfoPanel != null)
        {
            weaponInfoPanel.SetActive(isFirstPersonActive);
        }
        else
        {
            gameObject.SetActive(isFirstPersonActive);
        }

        if (isFirstPersonActive)
        {
            RefreshWeaponInfoDisplay();
        }
    }

    private void RefreshWeaponInfoDisplay()
    {
        WeaponData currentWeapon = WeaponManager.CurrentWeapon;

        if (currentWeapon != null)
        {
            UpdateWeaponNameDisplay(currentWeapon);
            UpdateAmmoDisplay();
        }
        else
        {
            ClearWeaponInfoDisplay();
        }
    }

    private void UpdateWeaponNameDisplay(WeaponData currentWeapon)
    {
        if (weaponNameText != null)
        {
            weaponNameText.text = currentWeapon.weaponName;
        }
    }

    private void UpdateAmmoDisplay()
    {
        if (shooterReference == null || WeaponManager.CurrentWeapon == null) return;

        WeaponData currentWeapon = WeaponManager.CurrentWeapon;
        bool isRangedWeapon = IsWeaponRanged(currentWeapon.weaponType);

        if (isRangedWeapon)
        {
            int currentMagazine = shooterReference.GetCurrentMagazineCount();
            int currentReserve = shooterReference.GetCurrentReserveMagazineCount();

            UpdateMagazineDisplay(currentMagazine, currentWeapon.magazineSize);
            UpdateClipDisplay(currentReserve);
        }
        else
        {
            // For melee weapons, show infinity or hide ammo display
            UpdateMagazineDisplay(-1, -1); // -1 indicates infinite/melee
            UpdateClipDisplay(-1);
        }
    }

    private void UpdateMagazineDisplay(int currentMagazine, int magazineSize)
    {
        if (currentMagazineText != null)
        {
            if (currentMagazine >= 0 && magazineSize >= 0)
            {
                currentMagazineText.text = $"{currentMagazine}/{magazineSize}";

                // Optional: Change color based on ammo level
                if (currentMagazine == 0)
                {
                    currentMagazineText.color = Color.red;
                }
                else if (currentMagazine <= magazineSize * 0.3f) // 30% or less
                {
                    currentMagazineText.color = Color.yellow;
                }
                else
                {
                    currentMagazineText.color = Color.white;
                }
            }
            else
            {
                currentMagazineText.text = "Infinite"; // Infinite for melee weapons
                currentMagazineText.color = Color.white;
            }
        }
    }

    private void UpdateClipDisplay(int currentReserve)
    {
        if (currentClipText != null)
        {
            if (currentReserve >= 0)
            {
                currentClipText.text = currentReserve.ToString();

                // Optional: Change color based on reserve ammo level
                if (currentReserve == 0)
                {
                    currentClipText.color = Color.red;
                }
                else if (currentReserve <= 10) // Low reserve threshold
                {
                    currentClipText.color = Color.yellow;
                }
                else
                {
                    currentClipText.color = Color.white;
                }
            }
            else
            {
                currentClipText.text = "Infinite"; // Infinite for melee weapons
                currentClipText.color = Color.white;
            }
        }
    }

    private void ClearWeaponInfoDisplay()
    {
        if (weaponNameText != null)
            weaponNameText.text = "No Weapon";

        if (currentMagazineText != null)
            currentMagazineText.text = "0/0";

        if (currentClipText != null)
            currentClipText.text = "0";
    }

    private bool IsWeaponRanged(WeaponData.WeaponType weaponType)
    {
        return weaponType == WeaponData.WeaponType.Pistol ||
               weaponType == WeaponData.WeaponType.Rifle ||
               weaponType == WeaponData.WeaponType.RPG;
    }
}