using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("Scriptable Objects/Narration/Dialogue/Node/Conditional Basic"))]
public class ConditionalBasicDialogueNode : BasicDialogueNode
{
    //Could also make this into an array
    public DialogueCondition[] conditions;

    public override bool NodeConditionsMet()
    {
        for (int i = 0; i < conditions.Length; i++)
        {
            if (conditions[i].ConditionMet() == false) return false;
        }
        return true;
    }
}