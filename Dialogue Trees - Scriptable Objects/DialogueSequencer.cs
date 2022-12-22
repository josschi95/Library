
/// <summary>
/// Controls the sequences of nodes and determiens if a new node or dialogue is able to start
/// This class sits within the DialogueInstigator class, which is likely held by the player
/// </summary>

public class DialogueSequencer
{
    public delegate void DialogueCallback(Dialogue dialogue);
    public delegate void DialogueNodeCallback(DialogueNode node);

    public DialogueCallback onDialogueStart;
    public DialogueCallback onDialogueEnd;
    public DialogueNodeCallback onDialogueNodeStart;
    public DialogueNodeCallback onDialogueNodeEnd;

    private Dialogue m_CurrentDialogue;
    private DialogueNode m_CurrentNode;

    //Initiates dialogue
    public void StartDialogue(Dialogue dialogue)
    {
        if (m_CurrentDialogue == null)
        {
            m_CurrentDialogue = dialogue;
            onDialogueStart?.Invoke(m_CurrentDialogue);
            StartDialogueNode(dialogue.FirstNode);
        }
        else
        {
            throw new DialogueException("Can't start a dialgoue when another is already running.");
        }
    }

    public void EndDialogue(Dialogue dialogue)
    {
        if (m_CurrentDialogue == dialogue)
        {
            StopDialogueNode(m_CurrentNode);
            onDialogueEnd?.Invoke(m_CurrentDialogue);
            m_CurrentDialogue = null;
        }
        else
        {
            throw new DialogueException("Trying to stop a dialogue that isn't running.");
        }
    }

    private bool CanStartNode(DialogueNode node)
    {
        return (m_CurrentNode == null || node == null || m_CurrentNode.CanBeFollowedByNode(node));
    }

    public void StartDialogueNode(DialogueNode node)
    {
        if (CanStartNode(node))
        {
            StopDialogueNode(m_CurrentNode);

            m_CurrentNode = node;

            if (m_CurrentNode != null)
            {
                onDialogueNodeStart?.Invoke(m_CurrentNode);
            }
            else
            {
                EndDialogue(m_CurrentDialogue);
            }
        }
        else
        {
            throw new DialogueException("Failed to start dialogue node.");
        }
    }

    public void StopDialogueNode(DialogueNode node)
    {
        if (m_CurrentNode == node)
        {
            onDialogueNodeEnd?.Invoke(node);
            m_CurrentNode = null;
        }
        else
        {
            throw new DialogueException("Trying to stop a node that isn't running");
        }
    }
}

public class DialogueException : System.Exception
{
    public DialogueException(string message)
        : base(message)
    {
    }
}