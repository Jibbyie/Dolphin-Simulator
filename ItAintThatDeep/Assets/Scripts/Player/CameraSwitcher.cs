using UnityEngine;
using Unity.Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Camera & Movement References")]
    [SerializeField] private CinemachineCamera isometricCamera;    // I set priority for the 2D isometric view
    [SerializeField] private CinemachineCamera firstPersonCamera;  // I set priority for the first-person view
    [SerializeField] private PlayerMovement isometricMovement;     // I enable/disable isometric movement
    [SerializeField] private FirstPersonController firstPersonMovement; // I enable/disable FPS movement
    [SerializeField] private Camera mainUnityCamera;              // I switch between orthographic and perspective
    [SerializeField] private GameObject firstPersonUI;            // I toggle the FPS UI elements

    public static bool IsFirstPersonActive { get; private set; }
    public static event System.Action<bool> OnFirstPersonToggled;

    private bool useFirstPerson = false;  // I track whether FPS mode is active

    private void Awake()
    {
        // I start the game in isometric mode by default
        SetFirstPersonMode(false);
    }

    private void Update()
    {
        // I listen for right-click to toggle between camera modes
        if (Input.GetMouseButtonDown(1))
        {
            useFirstPerson = !useFirstPerson;
            SetFirstPersonMode(useFirstPerson);
        }
    }

    /// <summary>
    /// I switch cameras, movement scripts, projection, UI, and keep the cursor locked and hidden.
    /// </summary>
    private void SetFirstPersonMode(bool enableFirstPerson)
    {
        IsFirstPersonActive = enableFirstPerson;

        // I adjust Cinemachine priorities to activate the correct camera
        isometricCamera.Priority = enableFirstPerson ? 10 : 20;
        firstPersonCamera.Priority = enableFirstPerson ? 20 : 10;

        // I enable or disable movement components based on mode
        isometricMovement.enabled = !enableFirstPerson;
        firstPersonMovement.enabled = enableFirstPerson;

        // I switch camera projection: ortho for iso, perspective for FPS
        mainUnityCamera.orthographic = !enableFirstPerson;

        // I ensure the cursor stays locked and invisible regardless of mode
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // I toggle the FPS-specific UI container
        if (firstPersonUI != null)
            firstPersonUI.SetActive(enableFirstPerson);

        // I broadcast the toggle event
        OnFirstPersonToggled?.Invoke(enableFirstPerson);
    }
}
