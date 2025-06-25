using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Rifle WeaponData")]
public class RifleWeaponData : WeaponData
{
    private void OnEnable()
    {
        weaponType = WeaponType.Rifle;
    }
}
