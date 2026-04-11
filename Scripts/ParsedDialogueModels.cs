using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParsedDialogueFlow
{
    public string flowId;
    public string startNodeId;
    public List<ParsedDialogueNode> nodes = new();

    // Metadata
    public string brandKey;
    public string customerTypeKey;
    public string seasonKey;
    public int flowNumber;

    public ParsedDialogueNode FindNode(string nodeId)
    {
        return nodes.Find(n => n.id == nodeId);
    }
}

[Serializable]
public class ParsedDialogueNode
{
    public string id;
    public List<DialogueLineData> lines = new();
    public List<DialogueChoiceDataEx> choices = new();
    public List<string> systemNotes = new();
    public List<string> conditions = new();

    public ParsedNodeKind kind = ParsedNodeKind.Normal;

    public bool isTerminal;
    public bool isSuccessEnd;
    public bool isFailEnd;
}

[Serializable]
public class DialogueLineData
{
    public DialogueSpeaker speaker;
    [TextArea(2, 5)]
    public string text;
    public bool hasRuntimeOrderTag;
}

[Serializable]
public class DialogueChoiceDataEx
{
    public string key;
    public string text;
    public string nextNodeId;
}

public enum DialogueSpeaker
{
    Customer,
    System
}

public enum ParsedNodeKind
{
    Normal,
    PriceInput,
    Service,
    Terminal
}