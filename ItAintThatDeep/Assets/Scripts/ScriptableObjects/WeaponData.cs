using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Base WeaponData")]
public class WeaponData : ScriptableObject
{
    public enum WeaponType
    {
        Slap,
        Melee,
        Pistol,
        Rifle,
        RPG
    }
    
    public enum DamageType
    {
        Slap,
        Melee,
        Pistol,
        Rifle,
        Explosive
    }

    [Header("Common Weapon Settings")]
    public string weaponName;
    public WeaponType weaponType;
    public DamageType damageType;
    public float damage;
    public AudioClip shootSFX;

    [Header("Distance and Rate/Reload Settings")]
    public float range = 5f;
    public float fireRate = 1f; 
    public float reloadTime = 2f;

    [Header("Ammo Settings")]
    public int magazineSize; // bullets per magazine
    public int clipSize; // total magazines carried

    [Tooltip("Play this when the player equips this weapon")]
    public AudioClip switchSFX;
}
