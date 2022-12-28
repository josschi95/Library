using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VariableContainer : ScriptableObject
{
    public List<DialogueVariable> variables = new List<DialogueVariable>();
}
