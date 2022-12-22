using UnityEngine;

/// <summary>
/// The character who is speaking
/// Can be the player, an NPC, a narrator, etc.
/// </summary>

[CreateAssetMenu(menuName = "Scriptable Objects/Narration/Character")]
public class NarrationCharacter : ScriptableObject
{
    [SerializeField] private string m_CharacterName;
    public string CharacterName => m_CharacterName;
}
