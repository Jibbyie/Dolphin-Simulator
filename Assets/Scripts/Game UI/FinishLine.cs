using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Make sure your player has the tag "Player"
        {
            FindObjectOfType<LevelTimer>().StopTimer();
            Debug.Log("Finish Line Crossed! Timer Stopped.");
        }
    }
}
