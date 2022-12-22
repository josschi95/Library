using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalChoiceDialogueNode : ChoiceDialogueNode
{
    public override bool NodeConditionsMet()
    {
        return false;
    }
}
