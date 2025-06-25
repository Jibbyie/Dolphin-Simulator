using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public string weaponName;
    [Header("Common Weapon Settings")]
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
    public int clipSize; // reserve bullets in the clip

    [Tooltip("Play this when the player equips this weapon")]
    public AudioClip switchSFX;

}
