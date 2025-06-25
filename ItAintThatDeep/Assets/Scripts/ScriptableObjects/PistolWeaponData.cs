using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Pistol WeaponData")]
public class PistolWeaponData : WeaponData
{

    private void OnEnable()
    {
        weaponType = WeaponType.Pistol;
    }
}
