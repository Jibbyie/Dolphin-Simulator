using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Melee WeaponData")]
public class MeleeWeaponData : WeaponData
{

    private void OnEnable()
    {
        weaponType = WeaponType.Melee;
    }
}
