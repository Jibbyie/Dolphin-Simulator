using UnityEngine;

// Tracks how many degrees you've spun (yaw) while slo-mo is active.
// Reset when slo-mo ends or when you consume the trickshot.
public class TrickshotSpinTracker : MonoBehaviour
{
    [Header("Yaw Source (player body that rotates left/right)")]
    [SerializeField] private Transform yawSource; // e.g., your player root used by FirstPersonCamera

    [Header("Spin Settings")]
    [SerializeField] private float requiredDegrees = 300f; // ~ full turn but forgiving
    [SerializeField] private bool requireSloMo = true;     // only count spins during slo-mo

    private float accumulatedDegrees;
    private float lastYaw;
    private bool initialized;

    private void Reset()
    {
        // Try to auto-find the player body from the camera if not set
        var cam = FindFirstObjectByType<FirstPersonCamera>();
        if (cam != null)
        {
            // FirstPersonCamera has a playerBody field; expose or assign it here if public.
            // If not public, assign yawSource in the Inspector.
        }
    }

    private void Update()
    {
        if (yawSource == null) return;

        // Only count while slo-mo is active (optional)
        if (requireSloMo && !SloMo.IsActive)
        {
            accumulatedDegrees = 0f;
            initialized = false;
            return;
        }

        float yaw = yawSource.eulerAngles.y;

        if (!initialized)
        {
            lastYaw = yaw;
            initialized = true;
            return;
        }

        // Add signed shortest-angle delta
        float delta = Mathf.DeltaAngle(lastYaw, yaw);
        accumulatedDegrees += Mathf.Abs(delta);
        lastYaw = yaw;
    }

    // Returns true and resets the accumulator if threshold met
    public bool ConsumeIfReady()
    {
        if (accumulatedDegrees >= requiredDegrees)
        {
            accumulatedDegrees = 0f;
            initialized = false;
            return true;
        }
        return false;
    }

    public void ForceReset()
    {
        accumulatedDegrees = 0f;
        initialized = false;
    }
}
