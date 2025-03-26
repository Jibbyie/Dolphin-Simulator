using System;
using System.Collections.Generic;

[Serializable]
public class DialogueNode
{
    public string message;
    public List<DialogueOption> options;
    public Action onSelected;

    public DialogueNode(string msg)
    {
        message = msg;
        options = new List<DialogueOption>();
    }
}

[Serializable]
public class DialogueOption
{
    public string text;
    public DialogueNode nextNode;
}
