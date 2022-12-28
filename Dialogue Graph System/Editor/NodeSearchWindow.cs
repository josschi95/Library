using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using UnityEditor.UIElements;


public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private DialogueGraphView graphView;
    private DialogueGraphWindow window;
    private Texture2D indentationIcon;

    //private List<DialogueContainer> storedFiles;
    //private bool node;

    public void Init(DialogueGraphWindow window, DialogueGraphView graphView)
    {
        this.graphView = graphView;
        this.window = window;

        //Indentation fix for search window as transparent
        indentationIcon = new Texture2D(1, 1);
        indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0)); //Set the alpha as well
        indentationIcon.Apply();
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        //GetNarratives();

        var tree = new List<SearchTreeEntry>
        {
            //To create a new node
            new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0),
            new SearchTreeGroupEntry(new GUIContent("Dialogue"), 1),
            new SearchTreeEntry(new GUIContent("Dialogue Node", indentationIcon))
            {
                userData = new DialogueNode(), level = 2
            },
            new SearchTreeEntry(new GUIContent("Player Node", indentationIcon))
            {
                userData = new DialogueNode(), level = 2
            },

            //new SearchTreeGroupEntry(new GUIContent("Open Narrative"), 0),
            /*new SearchTreeGroupEntry(new GUIContent("Open Narrative"), 1),
            new SearchTreeEntry(new GUIContent("Create New Narrative", indentationIcon))
            {
                //Create a new narrative
                level = 2
            }*/
            
        };

        /*for (int i = 0; i < storedFiles.Count; i++)
        {
            var t = new SearchTreeEntry(new GUIContent(storedFiles[i].name, indentationIcon))
            {
                level = 2
            };
            tree.Add(t); 
        }*/

        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        var worldPos = window.rootVisualElement.ChangeCoordinatesTo(window.rootVisualElement.parent, 
            context.screenMousePosition - window.position.position);
        var localMousePos = graphView.contentViewContainer.WorldToLocal(worldPos);

        //Create a player dialogue node
        if (SearchTreeEntry.name == "Player Node")
        {
            graphView.CreateNode("Dialogue Node", localMousePos, true);
            return true;

        }
        //Create and NPC dialogue node
        if (SearchTreeEntry.name == "Dialogue Node")
        {
            graphView.CreateNode("Dialogue Node", localMousePos);
            return true;

        }
        //Create and open new narrative
        /*else if (SearchTreeEntry.name == "Create New Narrative")
        {
            //Save current and create new
            Debug.Log("Creating new narrative");
            return true;
        }
        //Open existing narrative
        else if (storedFiles.Any(x => x.name == SearchTreeEntry.name))
        {
            Debug.Log("Opening " + SearchTreeEntry.name);
            window.OpenFile(SearchTreeEntry.name);
            return true;
        }*/

        return false;
    }

    /*private void GetNarratives()
    {
        storedFiles = new List<DialogueContainer>();

        string[] guids = AssetDatabase.FindAssets("t:" + typeof(DialogueContainer).Name);
        DialogueContainer[] containers = new DialogueContainer[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            containers[i] = AssetDatabase.LoadAssetAtPath<DialogueContainer>(path);
        }
        storedFiles.AddRange(containers);
    }*/
}
