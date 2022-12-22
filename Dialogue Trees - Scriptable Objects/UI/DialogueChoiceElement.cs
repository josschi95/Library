using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script should be placed on a UI Element prefab to be instantiated into the Choice Box
/// </summary>

public class DialogueChoiceElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_Choice;
    [SerializeField] private Button button;
    //[SerializeField] private DialogueChannel m_DialogueChannel;

    private DialogueNode m_ChoiceNextNode;

    public DialogueChoice Choice
    {
        set
        {
            m_Choice.text = value.ChoicePreview;
            m_ChoiceNextNode = value.ChoiceNode;
        }
    }

    private void Start()
    {
        button.onClick.AddListener(OnClick);
        //GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        DialogueManager.RaiseRequestDialogueNode(m_ChoiceNextNode);
        //m_DialogueChannel.RaiseRequestDialogueNode(m_ChoiceNextNode);
    }
}
