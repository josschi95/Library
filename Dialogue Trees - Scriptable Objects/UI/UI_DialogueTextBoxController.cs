using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Place this script the Main Dialogue Panel UI Element
/// Controls the display of dialogue
/// 
/// Will likely make changes in the future to be able to place this on/in the DialogueManager script
/// </summary>

public class UI_DialogueTextBoxController : MonoBehaviour, DialogueNodeVisitor
{
    private DialogueManager dialogueManager;

    [SerializeField] private Button continueButton;

    [SerializeField] private TextMeshProUGUI m_SpeakerText;
    [SerializeField] private TextMeshProUGUI m_DialogueText;

    [SerializeField] private RectTransform m_ChoicesBoxTransform;
    [SerializeField] private DialogueChoiceElement m_ChoiceControllerPrefab;

    private bool m_ListenToInput = false;
    private DialogueNode m_NextNode = null;

    private void Awake()
    {
        dialogueManager = DialogueManager.instance;

        dialogueManager.onDialogueNodeStart += OnDialogueNodeStart;
        dialogueManager.onDialogueNodeEnd += OnDialogueNodeEnd;

        gameObject.SetActive(false);
        m_ChoicesBoxTransform.gameObject.SetActive(false);

        continueButton.onClick.AddListener(ContinueDialogue);
    }

    private void OnDestroy()
    {
        dialogueManager.onDialogueNodeEnd -= OnDialogueNodeEnd;
        dialogueManager.onDialogueNodeStart -= OnDialogueNodeStart;
    }

    private void ContinueDialogue()
    {
        if (m_ListenToInput)
        {
            DialogueManager.RaiseRequestDialogueNode(m_NextNode);
        }
    }

    private void OnDialogueNodeStart(DialogueNode node)
    {
        gameObject.SetActive(true);

        m_DialogueText.text = node.Text;
        m_SpeakerText.text = node.Speaker.CharacterName;

        node.Accept(this);
    }

    private void OnDialogueNodeEnd(DialogueNode node)
    {
        m_NextNode = null;
        m_ListenToInput = false;
        m_DialogueText.text = "";
        m_SpeakerText.text = "";

        foreach (Transform child in m_ChoicesBoxTransform)
        {
            Destroy(child.gameObject);
        }

        gameObject.SetActive(false);
        m_ChoicesBoxTransform.gameObject.SetActive(false);
    }

    public void Visit(BasicDialogueNode node)
    {
        m_ListenToInput = true;
        m_NextNode = node.GetNextNode();
    }

    public void Visit(ChoiceDialogueNode node)
    {
        m_ChoicesBoxTransform.gameObject.SetActive(true);

        foreach (DialogueChoice choice in node.Choices)
        {
            DialogueChoiceElement newChoice = Instantiate(m_ChoiceControllerPrefab, m_ChoicesBoxTransform);
            newChoice.Choice = choice;
        }
    }
}
