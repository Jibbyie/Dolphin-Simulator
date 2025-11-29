using UnityEngine;

public enum DialogueEmotion
{
    Neutral,
    Positive,
    Negative
}

[System.Serializable]
public class DialogueNode
{
    [TextArea(2, 4)]
    public string npcLine;

    public DialogueEmotion emotion = DialogueEmotion.Neutral;

    public DialogueOption[] options;
}

[System.Serializable]
public class DialogueOption
{
    [TextArea(1, 3)]
    public string playerLine;

    public int nextNodeIndex = -1;
    public bool givesAura;
    public bool givesAmmo;
}
