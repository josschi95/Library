using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("Scriptable Objects/Narration/Dialogue/Node/Branching Basic"))]
public class BranchingBasicDialogueNode : BasicDialogueNode
{
    [Tooltip("The order in which they are placed will determine priority")]
    [SerializeField] private ConditionalBasicDialogueNode[] branchingNodes;

    public override bool CanBeFollowedByNode(DialogueNode node)
    {
        if (node == m_NextNode) return true;

        for (int i = 0; i < branchingNodes.Length; i++)
        {
            if (node == branchingNodes[i] && branchingNodes[i].NodeConditionsMet()) return true;
        }

        return false;
    }

    public override DialogueNode GetNextNode()
    {
        for (int i = 0; i < branchingNodes.Length; i++)
        {
            if (branchingNodes[i].NodeConditionsMet()) return branchingNodes[i];
        }

        return m_NextNode;
    }
}
