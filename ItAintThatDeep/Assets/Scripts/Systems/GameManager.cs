using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] int targetFrameRate = 60; // change to whatever you want

    private void Awake()
    {
        // I lock and hide the cursor globally at game start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // Quit application when pressing P
        if (Input.GetKeyDown(KeyCode.P))
        {
            Application.Quit();
        }
    }
}
