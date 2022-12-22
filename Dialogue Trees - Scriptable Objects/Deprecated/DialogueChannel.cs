using UnityEngine;

/// <summary>
/// This script is only used to Invoke callbacks
/// This script is also used to initiate dialogue through the RaiseRequestDialogue method
/// There will never need to be more than one of these S.O.'s
/// 
/// If that's the case... why don't I also merge this into DialogueManager
/// I really don't see a reason for this being a scriptableObject
/// Some testing is yet to be done. But as far as I can tell, I can get rid of this and merge all of it
/// Merge is done. leaving this script intact in case future issues arise
/// </summary>

[CreateAssetMenu(menuName = "Scriptable Objects/Narration/Dialogue/Dialogue Channel")]
public class DialogueChannel : ScriptableObject
{
    public delegate void DialogueCallback(Dialogue dialogue);
    public DialogueCallback onDialogueRequested;
    public DialogueCallback onDialogueStart;
    public DialogueCallback onDialogueEnd;

    public delegate void DialogueNodeCallback(DialogueNode node);
    public DialogueNodeCallback onDialogueNodeRequested;
    public DialogueNodeCallback onDialogueNodeStart;
    public DialogueNodeCallback onDialogueNodeEnd;

    public void RaiseRequestDialogue(Dialogue dialogue)
    {
        //onDialogueRequested?.Invoke(dialogue);
        throw new DialogueException("This class is deprecated.");
    }

    public void RaiseDialogueStart(Dialogue dialogue)
    {
        //onDialogueStart?.Invoke(dialogue);
        throw new DialogueException("This class is deprecated.");
    }

    public void RaiseDialogueEnd(Dialogue dialogue)
    {
        //onDialogueEnd?.Invoke(dialogue);
        throw new DialogueException("This class is deprecated.");
    }


    public void RaiseRequestDialogueNode(DialogueNode node)
    {
        //onDialogueNodeRequested?.Invoke(node);
        throw new DialogueException("This class is deprecated.");
    }

    public void RaiseDialogueNodeStart(DialogueNode node)
    {
        //onDialogueNodeStart?.Invoke(node);
        throw new DialogueException("This class is deprecated.");
    }

    public void RaiseDialogueNodeEnd(DialogueNode node)
    {
        //onDialogueNodeEnd?.Invoke(node);
        throw new DialogueException("This class is deprecated.");
    }
}
