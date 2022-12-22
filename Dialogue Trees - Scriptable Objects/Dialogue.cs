using UnityEngine;

/// <summary>
/// Starts dialogue by introducing the first node in the series
/// Likely to be a greeting
/// </summary>

[CreateAssetMenu(menuName = "Scriptable Objects/Narration/Dialogue/Dialogue")]
public class Dialogue : ScriptableObject
{
    [SerializeField] private DialogueNode m_firstNode;
    public DialogueNode FirstNode => m_firstNode;
}
