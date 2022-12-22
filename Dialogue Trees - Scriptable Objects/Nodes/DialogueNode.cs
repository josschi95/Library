using UnityEngine;


public abstract class DialogueNode : ScriptableObject
{
    //Merging the function of NarrationLine into the nodes
    //[SerializeField] private NarrationLine m_DialogueLine;
    //public NarrationLine DialogueLine => m_DialogueLine;

    [SerializeField] private NarrationCharacter m_Speaker;
    [TextArea(2,5)]
    [SerializeField] private string m_text;

    public NarrationCharacter Speaker => m_Speaker;
    public string Text => m_text;

    [Tooltip("Makes changes to Dialogue Variables when visited")]
    public DialogueAffector[] effects;

    public abstract bool NodeConditionsMet();
    public abstract bool CanBeFollowedByNode(DialogueNode node);
    public abstract void Accept(DialogueNodeVisitor visitor);
}
