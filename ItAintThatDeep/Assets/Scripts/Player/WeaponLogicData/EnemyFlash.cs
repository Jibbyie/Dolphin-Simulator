using System.Collections;
using UnityEngine;

public class EnemyFlash : MonoBehaviour, IHitReactable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField][Tooltip("Duration of the flash effect in seconds")] private float flashDuration = 0.1f;
    [SerializeField][Tooltip("Color to flash on hit")] private Color flashColor = Color.red;

    // Store the original color so we can restore it
    private Color originalColor;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        originalColor = spriteRenderer.color;
    }

    public void OnHit(RaycastHit hit)
    {
        StopAllCoroutines();
        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        // Set to flash color
        spriteRenderer.color = flashColor;

        // Wait for the flash duration
        yield return new WaitForSeconds(flashDuration);

        // Restore original color
        spriteRenderer.color = originalColor;
    }
}
