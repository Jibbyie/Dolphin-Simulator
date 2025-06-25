using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        // I lock and hide the cursor globally at game start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
