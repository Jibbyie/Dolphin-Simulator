using UnityEngine;

[RequireComponent(typeof(DamageReciever))]
public class RangeIndicator : MonoBehaviour
{
    [SerializeField] private GameObject indicatorUI;
    // I assign my little ring sprite or UI element here in the Inspector.

    private Transform playerCam;
    // I’ll cache the player’s camera transform so I can measure distance.

    // I safely get the current weapon’s range, or 0 if no weapon is equipped.
    private float weaponRange => WeaponManager.CurrentWeapon?.range ?? 0f;

    private void Awake()
    {
        // I grab the Main Camera’s transform at startup (this is our “player” position).
        playerCam = Camera.main.transform;

        // I make sure the indicator starts hidden.
        indicatorUI.SetActive(false);
    }

    private void Update()
    {
        // I only want to show range hints in first-person mode with a weapon in hand.
        if (!CameraSwitcher.IsFirstPersonActive || WeaponManager.CurrentWeapon == null)
        {
            indicatorUI.SetActive(false);
            return;
        }

        // I compute the straight-line distance from player to this enemy.
        float dist = Vector3.Distance(playerCam.position, transform.position);

        // I turn the indicator on if we’re within range, off otherwise.
        indicatorUI.SetActive(dist <= weaponRange);
    }
}
