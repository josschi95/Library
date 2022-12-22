using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueTesting : MonoBehaviour
{
    //[SerializeField] private DialogueChannel dialogueChannel;
    [Space]
    [SerializeField] private Button[] actorButtons;
    [SerializeField] private Dialogue[] dialogues;

    private void Start()
    {
        for (int i = 0; i < actorButtons.Length; i++)
        {
            int t = i;
            actorButtons[t].onClick.AddListener(delegate { InitiateDialogue(t); });
        }
    }

    private void InitiateDialogue(int actor)
    {
        //dialogueChannel.RaiseRequestDialogue(dialogues[actor]);
        DialogueManager.RaiseRequestDialogue(dialogues[actor]);
    }
}
