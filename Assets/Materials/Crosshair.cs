using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Settings")]
    public RectTransform crosshairRect;
    public Image crosshairImage;
    public Color normalColor = Color.white;
    public Color targetColor = Color.red;
    public float normalSize = 50f;
    public float expandedSize = 70f;

    private Camera mainCamera;
    private bool targetDetected = false;

    void Start()
    {
        mainCamera = Camera.main;

        // Make sure we have the necessary components
        if (crosshairRect == null)
        {
            crosshairRect = GetComponent<RectTransform>();
        }

        if (crosshairImage == null)
        {
            crosshairImage = GetComponent<Image>();
        }

        // Set initial size
        if (crosshairRect != null)
        {
            crosshairRect.sizeDelta = new Vector2(normalSize, normalSize);
        }
    }

    void Update()
    {
        // Check if we're aiming at a target
        CheckForTarget();

        // Update crosshair appearance
        UpdateCrosshair();
    }

    void CheckForTarget()
    {
        // Reset target detection
        targetDetected = false;

        // If we don't have a main camera, exit
        if (mainCamera == null) return;

        // Cast a ray from the center of the screen
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // If we hit something, check if it's a target
        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Check if we hit an enemy
            if (hit.collider.CompareTag("Enemy"))
            {
                targetDetected = true;
            }
        }
    }

    void UpdateCrosshair()
    {
        // Update color
        if (crosshairImage != null)
        {
            crosshairImage.color = targetDetected ? targetColor : normalColor;
        }

        // Update size
        if (crosshairRect != null)
        {
            float targetSize = targetDetected ? expandedSize : normalSize;
            crosshairRect.sizeDelta = Vector2.Lerp(
                crosshairRect.sizeDelta,
                new Vector2(targetSize, targetSize),
                Time.deltaTime * 10f
            );
        }
    }
}