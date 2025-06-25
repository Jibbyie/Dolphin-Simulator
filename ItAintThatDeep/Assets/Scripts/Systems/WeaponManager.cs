using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private List<WeaponData> availableWeapons = new List<WeaponData>();
    public static WeaponData CurrentWeapon { get; private set; }
    public static WeaponData DamageType { get; private set; }
    public static event System.Action<WeaponData> OnWeaponSwitched;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] WeaponData weaponData;
    private int currentIndex = 0;
    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        //currentIndex = 0;
        //EquipAtIndex(currentIndex);
    }

    private void Update()
    {
        if (!CameraSwitcher.IsFirstPersonActive)
            return;

        for (int i = 0; i < availableWeapons.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                EquipAtIndex(i);
            }
        }

        // (Optional) Mouse-wheel cycling:
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0) EquipAtIndex((currentIndex + 1) % availableWeapons.Count);
        if (scroll < 0) EquipAtIndex((currentIndex - 1 + availableWeapons.Count) % availableWeapons.Count);
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
