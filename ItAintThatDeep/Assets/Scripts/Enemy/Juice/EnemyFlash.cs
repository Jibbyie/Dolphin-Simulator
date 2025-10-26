using System.Collections;
using UnityEngine;

/*
Briefly flashes the sprite to flashColor on hit, then restores.
If EnemyHealthTint is present, it re-applies the health tint after the flash.
*/
public class EnemyFlash : MonoBehaviour, IHitReactable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;

    private Color revertColor;
    private Coroutine flashRoutine;
    private EnemyHealthTint healthTint;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        healthTint = GetComponent<EnemyHealthTint>();

        if (spriteRenderer != null)
        {
            revertColor = spriteRenderer.color;
        }
    }

    public void OnHit(RaycastHit hit)
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }

        revertColor = spriteRenderer.color;

        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);

        if (healthTint != null)
        {
            healthTint.ApplyHealthTint();
        }
        else
        {
            spriteRenderer.color = revertColor;
        }

        flashRoutine = null;
    }
}
