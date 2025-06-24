using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float damage = 10f;
    public float range = 20f;
    public AudioClip shootSFX;

    [Tooltip("Play this when the player equips this weapon")]
    public AudioClip switchSFX;
}
