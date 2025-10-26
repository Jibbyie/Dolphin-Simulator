using System.Collections;
using UnityEngine;

/*
Pauses gameplay briefly by setting Time.timeScale to 0 and waiting in real time.
Calls while a pause is already active are ignored.
*/
public class HitStop : MonoBehaviour
{
    private static bool isBusy;

    /*
    Triggers a brief pause. Default is 0.04 seconds (real time).
    */
    public static void Do(float time = 0.04f)
    {
        if (isBusy == true)
        {
            return;
        }

        GameObject go = new GameObject("HitStop");
        HitStop hs = go.AddComponent<HitStop>();
        hs.StartCoroutine(hs.Pause(time));
    }

    /*
    Sets timeScale to 0, waits in real time, then restores and cleans up.
    */
    private IEnumerator Pause(float seconds)
    {
        isBusy = true;

        float previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (seconds > 0f)
        {
            yield return new WaitForSecondsRealtime(seconds);
        }
        else
        {
            yield return null;
        }

        Time.timeScale = previousTimeScale;
        isBusy = false;

        Destroy(gameObject);
    }
}
