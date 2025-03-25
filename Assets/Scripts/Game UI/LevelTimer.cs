using UnityEngine;
using UnityEngine.UI;

public class LevelTimer : MonoBehaviour
{
    public Text timerText; // Reference to the UI Text for displaying time
    private float elapsedTime;
    private bool isTiming;

    void Start()
    {
        elapsedTime = 0f;
        isTiming = true; // Start the timer when the game starts
    }

    void Update()
    {
        if (isTiming)
        {
            elapsedTime += Time.deltaTime;
            timerText.text = FormatTime(elapsedTime);
        }
    }

    public void StopTimer()
    {
        isTiming = false;
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time % 60F);
        int milliseconds = Mathf.FloorToInt((time * 100F) % 100F);
        return $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }
}
