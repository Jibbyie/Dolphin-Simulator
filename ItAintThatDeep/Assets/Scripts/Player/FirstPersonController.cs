using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Player Movement Settings")]
    [SerializeField] private float movementSpeed = 5f;      // I control walking speed
    [SerializeField] private float lookSensitivity = 2f;    // I control mouse look sensitivity
    [SerializeField] private Transform cameraPivot;         // I rotate the camera horizontally

    private Rigidbody playerRigidbody;                      // I move the player using physics

    private void Start()
    {
        // I cache the Rigidbody for movement
        playerRigidbody = GetComponent<Rigidbody>();

        // I lock and hide the cursor when entering FPS mode
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // I rotate the player horizontally based on mouse X input
        float horizontalLook = Input.GetAxis("Mouse X") * lookSensitivity;
        transform.Rotate(Vector3.up * horizontalLook);
    }

    private void FixedUpdate()
    {
        // I read WASD/arrow input, convert to world movement, and apply via Rigidbody
        Vector3 inputDirection = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0,
            Input.GetAxisRaw("Vertical")
        ).normalized;

        Vector3 worldMovement = transform.TransformDirection(inputDirection) * movementSpeed;
        playerRigidbody.MovePosition(playerRigidbody.position + worldMovement * Time.fixedDeltaTime);
    }
}