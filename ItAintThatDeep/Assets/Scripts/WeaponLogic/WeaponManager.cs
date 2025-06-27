using UnityEngine;
using System;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [Header("Available Weapons")]
    [SerializeField] private List<WeaponData> weaponInventory = new List<WeaponData>();
    // I track the currently equipped weapon for all other systems
    public static WeaponData CurrentWeapon { get; private set; }

    // I notify subscribers when the player switches weapons
    public static event Action<WeaponData> OnWeaponSwitched;

    [Header("Audio on Weapon Switch")]
    [SerializeField] private AudioSource switchAudioSource;    // I play the equip sound here

    // I remember which index in the inventory is currently equipped
    private int equippedWeaponIndex = 0;

    private void Awake()
    {
        // I ensure I have an AudioSource to play switch sounds
        if (switchAudioSource == null)
            switchAudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // I default to the first weapon in the list
        equippedWeaponIndex = 0;
        EquipWeaponAtIndex(equippedWeaponIndex);
    }

    private void OnDestroy()
    {
        // I clear subscribers to avoid dangling references
        OnWeaponSwitched = null;
    }

    private void Update()
    {
        // I only handle weapon input in first-person mode
        if (!CameraSwitcher.IsFirstPersonActive)
            return;

        // Number key quick-select: 1 for first, 2 for second, etc.
        for (int index = 0; index < weaponInventory.Count; index++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + index))
            {
                EquipWeaponAtIndex(index);
                return; // I process only one equip per frame
            }
        }

        // Mouse wheel cycling: scroll up = next, scroll down = previous
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta > 0f)
        {
            EquipWeaponAtIndex((equippedWeaponIndex + 1) % weaponInventory.Count);
        }
        else if (scrollDelta < 0f)
        {
            int previousIndex = (equippedWeaponIndex - 1 + weaponInventory.Count) % weaponInventory.Count;
            EquipWeaponAtIndex(previousIndex);
        }
    }

    // I equip the weapon at the given inventory index, play sound, and broadcast
    private void EquipWeaponAtIndex(int index)
    {
        if (index < 0 || index >= weaponInventory.Count)
            return; // I ignore invalid indices

        equippedWeaponIndex = index;
        CurrentWeapon = weaponInventory[index];
        Debug.Log($"Equipped weapon: {CurrentWeapon.weaponName}");

        // I play the switch sound if assigned
        if (switchAudioSource != null && CurrentWeapon.switchSFX != null)
            switchAudioSource.PlayOneShot(CurrentWeapon.switchSFX);

        // I notify any systems watching for weapon changes
        OnWeaponSwitched?.Invoke(CurrentWeapon);
    }
}
