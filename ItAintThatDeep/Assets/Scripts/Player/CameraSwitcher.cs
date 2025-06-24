using UnityEngine;
using Unity.Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private CinemachineCamera isoCam;
    [SerializeField] private CinemachineCamera fpCam;
    [SerializeField] private PlayerMovement isoMovement;
    [SerializeField] private FirstPersonController fpMovement;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject firstPersonCanvas;
    public static bool IsFirstPersonActive { get; private set; }
    public static event System.Action<bool> OnFirstPersonToggled;

    private bool usingFP = false;

    void Awake()
    {
        SetMode(false); // Start in isometric mode
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // Right-click toggles mode
        {
            usingFP = !usingFP;
            SetMode(usingFP);
        }
    }
    private void SetMode(bool firstPerson)
    {
        IsFirstPersonActive = firstPerson;

        isoCam.Priority = firstPerson ? 10 : 20;
        fpCam.Priority = firstPerson ? 20 : 10;

        isoMovement.enabled = !firstPerson;
        fpMovement.enabled = firstPerson;

        mainCamera.orthographic = !firstPerson;

        Cursor.lockState = firstPerson ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !firstPerson;

        if (firstPersonCanvas != null)
            firstPersonCanvas.SetActive(firstPerson);

        OnFirstPersonToggled?.Invoke(firstPerson);
    }
}
