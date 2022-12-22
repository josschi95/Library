using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Narration/Dialogue/Node/Basic")]
public class BasicDialogueNode : DialogueNode
{
    [Space]
    [SerializeField] protected DialogueNode m_NextNode;
    public DialogueNode NextNode => m_NextNode;

    public override bool NodeConditionsMet()
    {
        return true;
    }

    public virtual DialogueNode GetNextNode()
    {
        return m_NextNode;
    }

    public override bool CanBeFollowedByNode(DialogueNode node)
    {
        return m_NextNode == node;
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
