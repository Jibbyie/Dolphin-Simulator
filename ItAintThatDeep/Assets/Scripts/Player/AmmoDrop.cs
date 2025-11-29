using UnityEngine;

public class AmmoDrop : MonoBehaviour
{
    [Header("Floating & Rotation")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.25f;
    public float rotateSpeed = 50f;

    [Header("Pickup FX")]
    public GameObject pickupVFX;
    public AudioClip pickupSFX;

    [Header("Ammo Given Per Weapon Type")]
    public int pistolAmmo = 5;
    public int rifleAmmo = 10;
    public int rpgAmmo = 1;

    private Vector3 startPos;
    private AudioSource audioSource;

    private void Awake()
    {
        startPos = transform.position;
        audioSource = FindFirstObjectByType<AudioSource>();
    }

    private void Update()
    {
        // Bobbing
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPos + new Vector3(0f, bob, 0f);

        // Rotation
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        var shooter = other.GetComponentInParent<FirstPersonShooter>();
        if (shooter == null) return;

        ApplyAmmo(shooter);

        // FX
        if (pickupVFX != null)
            Instantiate(pickupVFX, transform.position, Quaternion.identity);

        if (pickupSFX != null && audioSource != null)
            audioSource.PlayOneShot(pickupSFX);

        Destroy(gameObject);
    }

    private void ApplyAmmo(FirstPersonShooter shooter)
    {
        var wm = FindFirstObjectByType<WeaponManager>();
        foreach (var weapon in wm.GetAllWeapons())
        {
            if (!FirstPersonShooter.IsRangedWeapon(weapon.weaponType))
                continue;

            int amount = GetAmountForWeapon(weapon.weaponType);
            if (amount > 0)
                shooter.AddReserveAmmoToWeapon(weapon, amount);
        }
    }

    private int GetAmountForWeapon(WeaponData.WeaponType type)
    {
        switch (type)
        {
            case WeaponData.WeaponType.Pistol: return pistolAmmo;
            case WeaponData.WeaponType.Rifle: return rifleAmmo;
            case WeaponData.WeaponType.RPG: return rpgAmmo;
        }
        return 0;
    }
}
