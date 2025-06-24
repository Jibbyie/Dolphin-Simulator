using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log($"{name} says: Hey! Watch it!");
    }
}
