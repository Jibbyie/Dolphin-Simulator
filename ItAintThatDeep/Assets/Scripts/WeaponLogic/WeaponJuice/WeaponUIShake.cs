using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*
Brief UI shake when a weapon fires.
- Put this on the UI RectTransform that holds the first-person weapon/arms.
- If "target" is empty, it uses this object.
- Tweak amplitude and duration in the inspector.
*/
public class WeaponUIShake : MonoBehaviour
{
    [Header("Target UI")]
    [SerializeField] private RectTransform target;   // panel that holds the weapon/arms

    [Header("Shake Settings")]
    [SerializeField] private float amplitude = 16f;   // pixels
    [SerializeField] private float duration = 0.08f; // seconds

    private Vector2 basePos;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        if (target == null)
        {
            target = GetComponent<RectTransform>();
        }

        if (target != null)
        {
            basePos = target.anchoredPosition;
        }
    }

    private void OnEnable()
    {
        FirstPersonShooter.OnWeaponFired += OnWeaponFired; // raised in FireWeapon()
    }

    private void OnDisable()
    {
        FirstPersonShooter.OnWeaponFired -= OnWeaponFired;

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        if (target != null)
        {
            target.anchoredPosition = basePos;
        }
    }

    private void OnWeaponFired()
    {
        if (target == null)
        {
            return;
        }

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }

        shakeRoutine = StartCoroutine(ShakeOnce());
    }

    private IEnumerator ShakeOnce()
    {
        float t = 0f;

        while (t < duration)
        {
            t = t + Time.deltaTime;

            // Linear falloff from full amplitude to 0
            float falloff = 1f - (t / duration);
            if (falloff < 0f) falloff = 0f;

            // Small random offset inside a circle
            Vector2 rnd = Random.insideUnitCircle * amplitude * falloff;

            target.anchoredPosition = basePos + rnd;

            yield return null;
        }

        target.anchoredPosition = basePos;
        shakeRoutine = null;
    }
}
