using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private List<WeaponData> availableWeapons = new List<WeaponData>();
    public static WeaponData CurrentWeapon { get; private set; }
    public static WeaponData DamageType { get; private set; }
    public static event System.Action<WeaponData> OnWeaponSwitched;
    [SerializeField] private AudioSource audioSource;

    private int currentIndex = 0;
    [SerializeField] private WeaponData startingWeapon;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        currentIndex = 0;
        EquipAtIndex(currentIndex);
    }

    private void Update()
    {
        if (!CameraSwitcher.IsFirstPersonActive)
            return;

        // Cycle slots 1 & 2
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipAtIndex(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && availableWeapons.Count > 1)
        {
            EquipAtIndex(1);
        }
    }

        private void EquipAtIndex(int i)
    {
        if (i < 0 || i >= availableWeapons.Count) return; // there's no weapons to equip, return

        currentIndex = i;
        CurrentWeapon = availableWeapons[i];
        Debug.Log($"Equipped: {CurrentWeapon.weaponName}");

        if (audioSource != null && CurrentWeapon.switchSFX != null)
            audioSource.PlayOneShot(CurrentWeapon.switchSFX);

        OnWeaponSwitched?.Invoke(CurrentWeapon);
    }

}
