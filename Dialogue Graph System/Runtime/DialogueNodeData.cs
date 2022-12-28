using System;
using UnityEngine;
using System.Collections.Generic;


[Serializable]
public class DialogueNodeData
{
    public string Guid;
    public string dialogueText;
    public Vector2 position;
    public bool isPlayerNode;
    public List<DialogueVariable> conditions = new List<DialogueVariable>();
}
