using System;
using UnityEngine;

[CreateAssetMenu(menuName = ("Scriptable Objects/Narration/Dialogue/Node/Choice"))]
public class ChoiceDialogueNode : DialogueNode
{
    [SerializeField] private DialogueChoice[] m_Choices;
    public DialogueChoice[] Choices => m_Choices;

    public override bool NodeConditionsMet()
    {
        return true;
    }

    public override bool CanBeFollowedByNode(DialogueNode node)
    {
        for (int i = 0; i < m_Choices.Length; i++)
        {
            if (m_Choices[i].ChoiceNode == node) return true;
        }
        return false;
    }

    public override void Accept(DialogueNodeVisitor visitor)
    {
        visitor.Visit(this);

        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].OnPull();
        }
    }
}

[Serializable]
public class DialogueChoice
{
    //Again... I don't know why this exists separate from the string on the node that is already accessible
    //My best guess is that you can have a shortened version of the node text, e.g. Fallout 4
    [Tooltip("The text that will appear on the button")]
    [SerializeField] private string m_ChoicePreview; 
    [SerializeField] private DialogueNode m_ChoiceNode;

    public string ChoicePreview => m_ChoicePreview;
    public DialogueNode ChoiceNode => m_ChoiceNode;
}
