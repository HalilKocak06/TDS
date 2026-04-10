using System;
using UnityEngine;

[Serializable]
public class DialogueNodeData
{
    public string id;

    [TextArea(3,8)]
    public string npcText;

    public DialogueChoiceData[] choices;

    public bool isPriceNode;
    public bool isTerminal;

}