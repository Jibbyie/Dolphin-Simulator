using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Image

[RequireComponent(typeof(DolphinMovement))]
public class DolphinShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint; // Assign dolphin's fire point
    public float fireRate = 0.5f;
    [Tooltip("Maximum distance the aiming raycast will check.")]
    public float aimMaxDistance = 1000f;
    [Tooltip("Layers the aiming ray should hit (e.g., everything except Player, Projectiles, IgnoreRaycast layers).")]
    public LayerMask aimLayerMask = ~0; // Default: Hits everything

    [Header("UI")]
    [Tooltip("Assign the UI Image element for the crosshair here.")]
    public Image crosshairImage; // Assign the CrosshairImage UI element

    [Header("Audio Settings")]
    public AudioSource shootSound;
    public AudioSource aimStartSound;
    public AudioSource aimStopSound;

    // --- Public Aiming State ---
    public bool IsAiming { get; private set; } = false;

    private float nextFireTime = 0f;
    private bool wasAimingLastFrame = false;
    private DolphinMovement movementScript;
    private Camera mainCamera; // Cache the main camera

    void Start()
    {
        movementScript = GetComponent<DolphinMovement>();
        if (movementScript == null)
        {
            Debug.LogError("DolphinShooting requires DolphinMovement on the same GameObject.", this);
        }

        // Cache the main camera for efficiency
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("DolphinShooting could not find the main camera!", this);
            enabled = false; // Disable script if no camera
            return;
        }

        // Ensure crosshair is assigned and initially hidden
        if (crosshairImage != null)
        {
            crosshairImage.enabled = false;
        }
        else
        {
            Debug.LogWarning("Crosshair Image not assigned in DolphinShooting.", this);
        }
    }

    void Update()
    {
        // Prevent aiming/shooting if movement is frozen
        if (movementScript != null && !movementScript.IsMovementEnabled())
        {
            HandleAimingStateChange(false); // Force stop aiming visuals/state
            return;
        }

        bool currentAimInput = Input.GetMouseButton(1); // RMB held down
        HandleAimingStateChange(currentAimInput); // Update aiming state and visuals

        HandleShootingInput();
    }

    void HandleAimingStateChange(bool isTryingToAim)
    {
        // Set the aiming state
        IsAiming = isTryingToAim;

        // Toggle Crosshair Visibility
        if (crosshairImage != null && crosshairImage.enabled != IsAiming)
        {
            crosshairImage.enabled = IsAiming;
        }

        // Play Aim Start/Stop Sounds
        if (IsAiming && !wasAimingLastFrame)
        { // Started aiming this frame
            if (aimStartSound != null) aimStartSound.Play();
        }
        else if (!IsAiming && wasAimingLastFrame)
        { // Stopped aiming this frame
            if (aimStopSound != null) aimStopSound.Play();
        }
        wasAimingLastFrame = IsAiming; // Store state for next frame
    }

    void HandleShootingInput()
    {
        // SHOOT Condition: Must be AIMING and press LMB
        if (IsAiming && Input.GetMouseButtonDown(0) && Time.time >= nextFireTime) // LMB
        {
            ShootTowardsCrosshair(); // Use the new accurate shooting method
            nextFireTime = Time.time + fireRate;
        }
    }

    void ShootTowardsCrosshair()
    {
        if (projectilePrefab == null || firePoint == null || mainCamera == null)
        {
            Debug.LogError("Shooting prerequisites not met (Prefab, FirePoint, or Camera missing).");
            return;
        }

        // 1. Define the Ray from the center of the camera view
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // Center of screen
        RaycastHit hit;
        Vector3 targetPoint;

        // 2. Perform the Raycast
        if (Physics.Raycast(ray, out hit, aimMaxDistance, aimLayerMask))
        {
            // Hit something in the world - use the hit point as the target
            targetPoint = hit.point;
            // Debug.DrawLine(ray.origin, hit.point, Color.green, 1f); // Visualize hit
        }
        else
        {
            // Didn't hit anything within range - target a point far along the ray
            targetPoint = ray.origin + ray.direction * aimMaxDistance;
            // Debug.DrawLine(ray.origin, targetPoint, Color.red, 1f); // Visualize miss/far point
        }

        // 3. Calculate the direction from the fire point to the target point
        Vector3 directionToTarget = (targetPoint - firePoint.position).normalized;

        // 4. Instantiate the projectile at the fire point, rotated towards the target
        // Debug.Log($"Shooting towards: {targetPoint}"); // Helpful for testing
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(directionToTarget));
        proj.tag = "PlayerProjectile";

        // Optional: Ignore collision with self
        Collider projCollider = proj.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();
        if (projCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(projCollider, playerCollider);
        }

        // 5. Play Sound
        if (shootSound != null)
            shootSound.Play();
    }
}