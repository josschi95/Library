using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DialogueContainer))]
public class DialogueContainer_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DialogueContainer container = (DialogueContainer)target;

        if (GUILayout.Button("Open Graph"))
        {
            //Gets the window, and opens it, if closed 
            var window = EditorWindow.GetWindow<DialogueGraphWindow>();
            //Focuses on the DialogueGraph Window
            EditorWindow.FocusWindowIfItsOpen<DialogueGraphWindow>();
            //Opens the selected file
            window.OpenFile(container.name);
        }
    }
}
