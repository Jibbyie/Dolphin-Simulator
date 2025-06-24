using System;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonUIController : MonoBehaviour
{
    [Serializable] 
    private struct WeaponUISlot
    {
        public WeaponData weapon; // the so this icon belongs to
        public GameObject iconGameObject; // the image gameobject under canvas
    }

    [Header("Wiring: one entry per available weapon")]
    [SerializeField] private List<WeaponUISlot> slots;

    private void Awake()
    {
        SyncUI(WeaponManager.CurrentWeapon);
    }

    private void OnEnable()
    {
        WeaponManager.OnWeaponSwitched += SyncUI;
        CameraSwitcher.OnFirstPersonToggled += gameObject.SetActive;
        SyncUI(WeaponManager.CurrentWeapon);
    }

    private void OnDisable()
    {
        WeaponManager.OnWeaponSwitched -= SyncUI;
        CameraSwitcher.OnFirstPersonToggled -= gameObject.SetActive;
    }

    private void SyncUI(WeaponData current)
    {
        // Loop over all slots, enable only the matching image
        for(int i = 0; i < slots.Count; i++)
        {
            bool match = slots[i].weapon == current;
            slots[i].iconGameObject.SetActive(match);
        }
    }
}
