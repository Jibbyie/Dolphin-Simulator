using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/RPG WeaponData")]
public class RPGWeaponData : WeaponData
{
    public float explosionRadius = 20f;

    private void OnEnable()
    {
        weaponType = WeaponType.RPG;
    }
}
