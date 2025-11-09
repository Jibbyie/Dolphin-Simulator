using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 7f;

    [Header("Sprint")]
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] float sprintMultiplier = 1.6f;

    [Header("Super Jump")]
    [SerializeField] KeyCode jumpKey = KeyCode.F;
    [SerializeField] float superJumpImpulse = 12f;

    [Header("Ground Check")]
    // Reuse one float instead of many knobs: acts as the feet sphere radius.
    [SerializeField] float groundCheckRadius = 0.25f;
    // Start with Everything; once it works, narrow to your Ground layer in the Inspector.
    [SerializeField] LayerMask groundMask = ~0;

    Rigidbody rb;
    Collider col;

    bool jumpPressed;
    public static bool IsSprinting { get; private set; }
    public static bool IsGroundedNow { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Read input in Update (reliable timing)
        IsSprinting = Input.GetKey(sprintKey);
        if (Input.GetKeyDown(jumpKey))
            jumpPressed = true;
    }

    void FixedUpdate()
    {
        // 1) Cache grounded ONCE at the start of physics step
        bool grounded = IsGrounded();
        IsGroundedNow = grounded;

        // 2) Movement (horizontal only; never stomp Y)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 moveDir = (transform.forward * moveZ + transform.right * moveX).normalized;

        float speed = moveSpeed * (IsSprinting ? sprintMultiplier : 1f);
        Vector3 v = rb.linearVelocity;
        v.x = moveDir.x * speed;
        v.z = moveDir.z * speed;
        rb.linearVelocity = v;

        // 3) Super jump: simple and strict
        if (jumpPressed && IsSprinting && grounded)
        {
            // clear any downward drift before we boost
            v = rb.linearVelocity;
            if (v.y < 0f) v.y = 0f;
            rb.linearVelocity = v;

            rb.AddForce(Vector3.up * superJumpImpulse, ForceMode.VelocityChange);
            // Debug.Log("Super jump!"); // optional
        }

        jumpPressed = false; // consume this physics tick
    }

    bool IsGrounded()
    {
        // Feet = collider bottom, slightly inside the body
        Bounds b = col.bounds;
        Vector3 feet = new Vector3(b.center.x, b.min.y + 0.05f, b.center.z);

        // Forgiving overlap so slopes/steps count
        return Physics.CheckSphere(feet, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
    }
}