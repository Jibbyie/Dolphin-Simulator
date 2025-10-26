using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
Shows a random "comic burst" sprite on the HUD whenever a weapon fires.
- Picks a random sprite from a list.
- Places it at a random position inside the container with padding.
- Scales it randomly (optional).
- Fades it out over a short duration, then destroys it.
*/
public class FirstPersonComicBursts : MonoBehaviour
{
    [Header("Container (this object if left empty)")]
    [SerializeField] private RectTransform burstsContainer;

    [Header("Burst Sprites")]
    [SerializeField] private List<Sprite> burstSprites = new List<Sprite>();

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 0.6f;          // seconds on screen
    [SerializeField] private float fadeTime = 0.2f;          // end-of-life fade duration

    [Header("Size (pixels)")]
    [SerializeField] private float minSize = 160f;
    [SerializeField] private float maxSize = 220f;

    [Header("Screen Padding (pixels)")]
    [SerializeField] private float padding = 40f;

    private void Awake()
    {
        if (burstsContainer == null)
        {
            burstsContainer = GetComponent<RectTransform>();
        }
    }

    private void OnEnable()
    {
        FirstPersonShooter.OnWeaponFired += HandleWeaponFired; // raised in FireWeapon()
    }

    private void OnDisable()
    {
        FirstPersonShooter.OnWeaponFired -= HandleWeaponFired;
    }

    private void HandleWeaponFired()
    {
        if (burstsContainer == null)
        {
            return;
        }

        if (burstSprites == null)
        {
            return;
        }

        if (burstSprites.Count == 0)
        {
            return;
        }

        // Pick a random sprite
        int index = Random.Range(0, burstSprites.Count);
        Sprite sprite = burstSprites[index];
        if (sprite == null)
        {
            return;
        }

        // Build a new Image under the container
        GameObject go = new GameObject("ComicBurst", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(burstsContainer, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        Image img = go.GetComponent<Image>();

        img.sprite = sprite;
        img.raycastTarget = false; // do not block UI clicks

        // Random size
        float size = Random.Range(minSize, maxSize);
        rt.sizeDelta = new Vector2(size, size);

        // Random position inside the panel with padding
        Vector2 pos = RandomAnchoredPosition(burstsContainer, rt.sizeDelta, padding);
        rt.anchoredPosition = pos;

        // Optional: slight random rotation so repeated bursts feel different
        float zRot = Random.Range(-15f, 15f);
        rt.localRotation = Quaternion.Euler(0f, 0f, zRot);

        // Start life+fade
        StartCoroutine(BurstLifeRoutine(img, lifetime, fadeTime));
    }

    private IEnumerator BurstLifeRoutine(Image img, float life, float fade)
    {
        // Stay fully visible for (life - fade)
        float solidTime = life - fade;
        if (solidTime < 0f)
        {
            solidTime = 0f;
        }

        if (solidTime > 0f)
        {
            yield return new WaitForSeconds(solidTime);
        }

        // Fade to transparent
        float t = 0f;
        Color start = img.color;
        Color end = start;
        end.a = 0f;

        if (fade <= 0f)
        {
            img.color = end;
        }
        else
        {
            while (t < fade)
            {
                t = t + Time.deltaTime;
                float alpha = t / fade;
                if (alpha > 1f)
                {
                    alpha = 1f;
                }

                img.color = Color.Lerp(start, end, alpha);
                yield return null;
            }
        }

        Destroy(img.gameObject);
    }

    // Chooses a random anchored position fully inside the container rect (with padding)
    private Vector2 RandomAnchoredPosition(RectTransform container, Vector2 elementSize, float pad)
    {
        // Container rect (in its own local/anchored space)
        Rect rect = container.rect;

        // Limits so the element stays fully inside
        float halfW = elementSize.x * 0.5f;
        float halfH = elementSize.y * 0.5f;

        float minX = rect.xMin + pad + halfW;
        float maxX = rect.xMax - pad - halfW;
        float minY = rect.yMin + pad + halfH;
        float maxY = rect.yMax - pad - halfH;

        // If padding or size is too large, clamp so we still place something
        if (minX > maxX) { float midX = (rect.xMin + rect.xMax) * 0.5f; minX = midX; maxX = midX; }
        if (minY > maxY) { float midY = (rect.yMin + rect.yMax) * 0.5f; minY = midY; maxY = midY; }

        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);

        return new Vector2(x, y);
    }
}
