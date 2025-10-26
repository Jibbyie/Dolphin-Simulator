using UnityEngine;

/*
Simple head-bob and strafe roll.
*/
public class CameraHeadBobTilt : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float bobAmp = 0.05f;
    [SerializeField] private float bobFreq = 12f;
    [SerializeField] private float strafeRoll = 4f;

    [Header("Sprint Scaling")]
    [SerializeField] private float sprintAmpScale = 1.5f;
    [SerializeField] private float sprintFreqScale = 1.2f;

    private Vector3 baseLocalPos;
    private float bobPhase;

    public float BobAmplitude { get { return bobAmp; } }
    public float BobFrequency { get { return bobFreq; } }
    public float StrafeRoll { get { return strafeRoll; } }

    private void Start()
    {
        baseLocalPos = transform.localPosition;
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        bool moving = Mathf.Abs(x) + Mathf.Abs(z) > 0.1f;

        // Effective params (boost if sprinting)
        float amp = bobAmp;
        float freq = bobFreq;
        if (FirstPersonController.IsSprinting)
        {
            amp *= sprintAmpScale;
            freq *= sprintFreqScale;
        }

        if (moving)
            bobPhase += Time.deltaTime * freq;
        else
            bobPhase = Mathf.Lerp(bobPhase, 0f, Time.deltaTime * 10f);

        float moveMul = moving ? 1f : 0f;

        float y = Mathf.Sin(bobPhase) * amp * moveMul;
        transform.localPosition = baseLocalPos + new Vector3(0f, y, 0f);

        float rollMul = moving ? 1f : 0f;
        float roll = -x * strafeRoll * rollMul;
        transform.localRotation = Quaternion.Euler(0f, 0f, roll);
    }
}
