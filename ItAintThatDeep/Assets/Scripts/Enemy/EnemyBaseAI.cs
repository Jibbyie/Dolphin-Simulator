using UnityEngine;

public class EnemyBaseAI : MonoBehaviour
{
    private bool stunned = false;
    public bool IsStunned => stunned;

    public void SetStunned(bool val)
    {
        stunned = val;
    }
}
