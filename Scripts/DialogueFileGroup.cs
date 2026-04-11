using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueFileGroup
{
    public string groupName;
    public List<TextAsset> files = new();
}
