using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple center-screen crosshair that lights up when your current shot
/// would actually deal damage if you fired right now. Additionally swaps
/// the crosshair sprite depending on the equipped weapon.
/// </summary>
[DisallowMultipleComponent]
public class CrosshairController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FirstPersonShooter shooter; // drag from scene
    [SerializeField] private Image reticle;              // the UI Image at screen center

    [Header("Reticle Sprites")]
    [Tooltip("Sprite used when no weapon-specific sprite is assigned.")]
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite rifleSprite;
    [SerializeField] private Sprite rpgSprite;
    [SerializeField] private Sprite pistolSprite;

    [Header("Colors")]
    [SerializeField] private Color idleColor = Color.white;    // default
    [SerializeField] private Color lockColor = Color.green;    // on valid target
    [SerializeField] private Color blockedColor = Color.red;   // optional (e.g., reloading/out of ammo)

    [Header("Options")]
    [Tooltip("Hide the reticle when there is no weapon equipped.")]
    [SerializeField] private bool hideWhenNoWeapon = false;

    // Cache to avoid per-frame GC
    private RaycastHit _hit;

    private void Reset()
    {
        // Try to auto-wire in editor
        if (reticle == null) reticle = GetComponentInChildren<Image>(true);
        if (shooter == null) shooter = FindFirstObjectByType<FirstPersonShooter>();
    }

    private void Awake()
    {
        if (reticle == null)
            Debug.LogWarning("CrosshairController: Please assign a reticle Image.");
        if (shooter == null)
            shooter = FindFirstObjectByType<FirstPersonShooter>();
    }

    private void OnEnable()
    {
        // Ensure we start in a sane visual state
        SetReticleColor(idleColor);
        ApplySpriteForWeapon(null); // apply default sprite at start
    }

    private void Update()
    {
        var weapon = WeaponManager.CurrentWeapon; // current equipped weapon
        if (reticle == null)
            return;

        // Optional visibility when no weapon
        if (weapon == null)
        {
            if (hideWhenNoWeapon)
            {
                reticle.enabled = false;
            }
            else
            {
                reticle.enabled = true;
                SetReticleColor(idleColor);
                ApplySpriteForWeapon(null); // default sprite if no weapon
            }
            return;
        }

        reticle.enabled = true;

        // Update sprite based on weapon
        ApplySpriteForWeapon(weapon);

        // If you want the reticle to go "blocked" while reloading or out of ammo,
        // uncomment this block (cosmetic only; shooting logic still unchanged).
        /*
        if (WeaponStateTracker.CurrentWeaponState == WeaponStateTracker.WeaponState.Reloading ||
            WeaponStateTracker.CurrentWeaponState == WeaponStateTracker.WeaponState.OutOfAmmo)
        {
            SetReticleColor(blockedColor);
            return;
        }
        */

        bool wouldDealDamage = WouldDealDamageNow(weapon);

        SetReticleColor(wouldDealDamage ? lockColor : idleColor);
    }

    private void SetReticleColor(Color c)
    {
        if (reticle != null) reticle.color = c;
    }

    /// <summary>
    /// Chooses a sprite for the reticle based on the provided weapon.
    /// If weapon is null or no matching sprite assigned, falls back to defaultSprite.
    /// If defaultSprite is also null, keeps whatever sprite is already on the Image.
    /// </summary>
    /// <param name="weapon">Current weapon (may be null)</param>
    private void ApplySpriteForWeapon(WeaponData weapon)
    {
        if (reticle == null) return;

        Sprite chosen = null;

        if (weapon != null)
        {
            switch (weapon.weaponType)
            {
                case WeaponData.WeaponType.Rifle:
                    chosen = rifleSprite;
                    break;
                case WeaponData.WeaponType.RPG:
                    chosen = rpgSprite;
                    break;
                case WeaponData.WeaponType.Pistol:
                    chosen = pistolSprite;
                    break;
                default:
                    chosen = defaultSprite;
                    break;
            }
        }
        else
        {
            // No weapon equipped
            chosen = defaultSprite;
        }

        // If we didn't find a sprite to apply, don't overwrite the existing sprite.
        if (chosen != null && reticle.sprite != chosen)
            reticle.sprite = chosen;
    }

    /// <summary>
    /// Mirrors your actual fire queries:
    /// - Pistol/Rifle: Physics.Raycast from the same camera, same mask, same range.
    /// - RPG: Physics.SphereCastAll with the same radius & range, ignoring the player.
    /// Also respects DamageType immunities so “lock” only shows if damage would apply.
    /// </summary>
    private bool WouldDealDamageNow(WeaponData weapon)
    {
        if (shooter == null || shooter.FirstPersonCamera == null || weapon == null)
            return false;

        Camera cam = shooter.FirstPersonCamera;
        Vector3 origin = cam.transform.position;
        Vector3 dir = cam.transform.forward;
        float range = weapon.range;
        LayerMask mask = shooter.HittableLayers;

        // Non-RPG: single hitscan ray like your Fire logic
        if (weapon.weaponType != WeaponData.WeaponType.RPG)
        {
            if (Physics.Raycast(origin, dir, out _hit, range, mask))
            {
                // Require a DamageReciever and check immunity like PerformDamageRaycast does
                if (_hit.collider.TryGetComponent<DamageReciever>(out var receiver))
                {
                    // Skip locking if immune to this weapon's damage type
                    return !receiver.DamageTypeImmunities.Contains(weapon.damageType);
                }
            }
            return false;
        }

        // RPG path: mirror your SphereCastAll (splash along the ray)
        Ray attackRay = new Ray(origin, dir);
        RaycastHit[] hits = Physics.SphereCastAll(
            attackRay,
            weapon.sphereCastRadius,
            range,
            mask,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];

            // Ignore the player object (same as your RPG code does using 'gameObject')
            if (shooter != null && h.collider.gameObject == shooter.gameObject)
                continue;

            if (h.collider.TryGetComponent<DamageReciever>(out var receiver))
            {
                if (!receiver.DamageTypeImmunities.Contains(weapon.damageType))
                    return true;
            }
        }

        return false;
    }
}
