using UnityEngine;

public class DialogueInstigator : MonoBehaviour
{
    private DialogueManager dialogueManager;

    //[SerializeField] private DialogueChannel m_DialogueChannel;
    //[SerializeField] private FlowChannel m_FlowChannel;
    //[SerializeField] private FlowState m_DialogueState;

    private DialogueSequencer m_DialogueSequencer;
    //private FlowState m_CachedFlowState;

    private void Awake()
    {
        dialogueManager = DialogueManager.instance;
        m_DialogueSequencer = new DialogueSequencer();

        m_DialogueSequencer.onDialogueStart += OnDialogueStart;
        m_DialogueSequencer.onDialogueEnd += OnDialogueEnd;

        m_DialogueSequencer.onDialogueNodeStart += DialogueManager.RaiseDialogueNodeStart;
        m_DialogueSequencer.onDialogueNodeEnd += DialogueManager.RaiseDialogueNodeEnd;
        //m_DialogueSequencer.onDialogueNodeStart += m_DialogueChannel.RaiseDialogueNodeStart;
        //m_DialogueSequencer.onDialogueNodeEnd += m_DialogueChannel.RaiseDialogueNodeEnd;

        dialogueManager.onDialogueRequested += m_DialogueSequencer.StartDialogue;
        dialogueManager.onDialogueNodeRequested += m_DialogueSequencer.StartDialogueNode;
        //m_DialogueChannel.onDialogueRequested += m_DialogueSequencer.StartDialogue;
        //m_DialogueChannel.onDialogueNodeRequested += m_DialogueSequencer.StartDialogueNode;
    }

    private void OnDestroy()
    {
        dialogueManager.onDialogueNodeRequested -= m_DialogueSequencer.StartDialogueNode;
        dialogueManager.onDialogueRequested -= m_DialogueSequencer.StartDialogue;
        //m_DialogueChannel.onDialogueNodeRequested -= m_DialogueSequencer.StartDialogueNode;
        //m_DialogueChannel.onDialogueRequested -= m_DialogueSequencer.StartDialogue;

        m_DialogueSequencer.onDialogueNodeEnd -= DialogueManager.RaiseDialogueNodeEnd;
        m_DialogueSequencer.onDialogueNodeStart -= DialogueManager.RaiseDialogueNodeStart;
        //m_DialogueSequencer.onDialogueNodeEnd -= m_DialogueChannel.RaiseDialogueNodeEnd;
        //m_DialogueSequencer.onDialogueNodeStart -= m_DialogueChannel.RaiseDialogueNodeStart;

        m_DialogueSequencer.onDialogueEnd -= OnDialogueEnd;
        m_DialogueSequencer.onDialogueStart -= OnDialogueStart;

        m_DialogueSequencer = null;
    }

    private void OnDialogueStart(Dialogue dialogue)
    {
        DialogueManager.RaiseDialogueStart(dialogue);
        //m_DialogueChannel.RaiseDialogueStart(dialogue);

        //m_CachedFlowState = FlowStateMachine.Instance.CurrentState;
        //m_FlowChannel.RaiseFlowStateRequest(m_DialogueState);
    }

    private void OnDialogueEnd(Dialogue dialogue)
    {
        //m_FlowChannel.RaiseFlowStateRequest(m_CachedFlowState);
        //m_CachedFlowState = null;

        DialogueManager.RaiseDialogueEnd(dialogue);
        //m_DialogueChannel.RaiseDialogueEnd(dialogue);
    }
}
