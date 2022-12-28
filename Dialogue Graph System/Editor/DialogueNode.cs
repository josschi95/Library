using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

public class DialogueNode : Node
{
    public string GUID;

    public string dialogueText;

    public bool entryPoint = false;

    public bool isPlayerNode = false;

    public TextField dialogueTextField { get; private set; }
    public List<DialogueVariable> conditions = new List<DialogueVariable>();
    //Conditions

    public void SetTextField(TextField field)
    {
        dialogueTextField = field;
    }

    public override void OnSelected()
    {
        DialogueInspector.OnNodeSelected(this);
        base.OnSelected();
    }

    public override void OnUnselected()
    {
        DialogueInspector.OnNodeUnselected();
        base.OnUnselected();
    }

    public void Refresh()
    {
        RefreshExpandedState();
        RefreshPorts();
    }
}