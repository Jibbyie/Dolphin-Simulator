using UnityEngine;

public class CursorLocker : MonoBehaviour
{
    void Start()
    {
        LockCursor();
    }

    void Update()
    {
        // In editor, lock can be lost when clicking outside
        if (Cursor.lockState != CursorLockMode.Locked)
            LockCursor();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            LockCursor();
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
