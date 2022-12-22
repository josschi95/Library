using UnityEngine;

/// <summary>
/// This is the text that will appear in the dialogue box during conversation
/// Each instance of this is a separate line
/// 
/// I really don't know why this is separate from DialogueNodes
/// If I find that I can merge them, I'm definitely going to
/// I merged them. This class is useless now but I'll keep it around in case
/// </summary>

[CreateAssetMenu(menuName = "Scriptable Objects/Narration/Line")]
public class NarrationLine : ScriptableObject
{
    [SerializeField] private NarrationCharacter m_speaker;
    [SerializeField] private string m_text;

    public NarrationCharacter Speaker => m_speaker;
    public string Text => m_text;
}