using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Player Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Sprint")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private float sprintMultiplier = 1.6f;

    private Rigidbody rb;

    // Simple global read so other scripts (bob/shake) can react to sprint
    public static bool IsSprinting { get; private set; }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = transform.forward * moveZ + transform.right * moveX;
        moveDir.Normalize();

        // Sprint only when there is movement input
        bool wantsSprint = Input.GetKey(sprintKey);
        bool hasInput = Mathf.Abs(moveX) + Mathf.Abs(moveZ) > 0.01f;
        IsSprinting = wantsSprint && hasInput;

        float speed = moveSpeed * (IsSprinting ? sprintMultiplier : 1f);
        rb.MovePosition(rb.position + moveDir * speed * Time.fixedDeltaTime);
    }
}
