using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _speed = 5;
    [SerializeField] private float _turnSpeed = 360;
    private Vector3 _input;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] directionalSprites; // Order: N, NE, E, SE, S, SW, W, NW


    private void Update()
    {
        GatherInput();
        Look();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void GatherInput()
    {
        _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        UpdateSpriteDirection();
    }

    private void Look()
    {
        if (_input == Vector3.zero) return;

        var rot = Quaternion.LookRotation(_input.ToIso(), Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, _turnSpeed * Time.deltaTime);
    }

    private void Move()
    {
        _rb.MovePosition(transform.position + transform.forward * _input.normalized.magnitude * _speed * Time.deltaTime);
    }

    private void UpdateSpriteDirection()
    {
        if (_input == Vector3.zero) return;

        Vector3 isoDir = _input.ToIso().normalized;
        float angle = Mathf.Atan2(isoDir.x, isoDir.z) * Mathf.Rad2Deg;
        angle = (angle + 360) % 360;

        int directionIndex = Get4DirIndex(angle);

        if (directionalSprites != null && directionalSprites.Length == 4)
            spriteRenderer.sprite = directionalSprites[directionIndex];
    }

    private int Get4DirIndex(float angle)
    {
        if (angle >= 315f || angle < 45f) return 0;   // North
        if (angle >= 45f && angle < 135f) return 1;   // East
        if (angle >= 135f && angle < 225f) return 2;  // South
        return 3;                                     // West
    }


}

public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}