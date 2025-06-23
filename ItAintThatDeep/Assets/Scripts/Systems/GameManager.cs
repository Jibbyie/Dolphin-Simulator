using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        Cursor.visible = false;
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
    }
}
