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

    [Header("Weapon Identification")]
    public Sprite weaponIcon;
    public string weaponName;

    [Header("Common Weapon Settings")]
    public WeaponType weaponType;
    public DamageType damageType;
    public float damage;
    public AudioClip shootSFX;

    [Header("RPG Settings (only used for WeaponType.RPG)")]
    [Tooltip("Radius of the sphere cast for RPG splash damage")]
    public float sphereCastRadius = 5f;

    [Header("Distance and Rate/Reload Settings")]
    public float range = 5f;
    public float fireRate = 1f; 
    public float reloadTime = 2f;

    [Header("Ammo Settings")]
    public int magazineSize; // bullets per magazine
    public int clipSize; // reserve bullets in the clip

    [Header("Audio Settings")]
    [Tooltip("Play this when the player equips this weapon")]
    public AudioClip switchSFX;

    [Tooltip("Play this when the player reloads this weapon")]
    public AudioClip reloadSFX;

    [Tooltip("Play this when the clip is empty")]
    public AudioClip emptySFX;

}
