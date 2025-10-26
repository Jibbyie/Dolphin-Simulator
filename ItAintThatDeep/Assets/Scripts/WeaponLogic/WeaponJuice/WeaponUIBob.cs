using UnityEngine;

/*
Applies the same head-bob and strafe roll to a UI weapon panel.
- Put this on the RectTransform that holds the weapon/arms UI.
- If "copyFromCamera" is true, it mirrors CameraHeadBobTilt values.
- Otherwise, you can tune bob settings locally in the inspector.
*/
[RequireComponent(typeof(RectTransform))]
public class WeaponUIBob : MonoBehaviour
{
    [Header("Source (optional)")]
    [SerializeField] private CameraHeadBobTilt cameraBob;
    [SerializeField] private bool copyFromCamera = true;

    [Header("Local Settings (used if copyFromCamera = false)")]
    [SerializeField] private float bobAmp = 6f;     // pixels up/down
    [SerializeField] private float bobFreq = 12f;   // cycles per second
    [SerializeField] private float strafeRoll = 4f; // degrees roll on strafe

    private RectTransform rt;
    private Vector2 basePos;
    private float bobPhase;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        basePos = rt.anchoredPosition;
    }

    private void OnEnable()
    {
        // Reset to a known base when enabled
        rt.anchoredPosition = basePos;
        rt.localRotation = Quaternion.identity;
    }

    private void OnDisable()
    {
        // Restore position and rotation
        rt.anchoredPosition = basePos;
        rt.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        // Read inputs (same as camera uses)
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        bool moving = false;
        if (Mathf.Abs(x) + Mathf.Abs(z) > 0.1f)
        {
            moving = true;
        }

        // Choose parameters (mirror camera or use local)
        float amp;
        float freq;
        float rollDeg;

        if (copyFromCamera == true && cameraBob != null)
        {
            amp = cameraBob.BobAmplitude * 100f; // convert meters-ish to pixels (tweak if needed)
            freq = cameraBob.BobFrequency;
            rollDeg = cameraBob.StrafeRoll;
        }
        else
        {
            amp = bobAmp;
            freq = bobFreq;
            rollDeg = strafeRoll;
        }

        // Advance phase only while moving
        if (moving == true)
        {
            bobPhase = bobPhase + Time.deltaTime * freq;
        }
        else
        {
            bobPhase = Mathf.Lerp(bobPhase, 0f, Time.deltaTime * 10f);
        }

        float moveMul = 0f;
        if (moving == true)
        {
            moveMul = 1f;
        }

        // Vertical bob in pixels
        float y = Mathf.Sin(bobPhase) * amp * moveMul;
        rt.anchoredPosition = new Vector2(basePos.x, basePos.y + y);

        // Strafe roll in degrees
        float rollMul = 0f;
        if (moving == true)
        {
            rollMul = 1f;
        }

        float roll = -x * rollDeg * rollMul;
        rt.localRotation = Quaternion.Euler(0f, 0f, roll);
    }
}
