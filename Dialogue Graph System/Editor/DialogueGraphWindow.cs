using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;

public class DialogueGraphWindow : EditorWindow
{
    public DialogueGraphView graphView;
    private DialogueInspector dialogueInspector;

    public string fileName = "New Narrative";
    private MiniMap miniMap;

    [MenuItem("Dialogue/Dialogue Graph")]
    public static void OpenDialogueGraphWindow()
    {
        var window = GetWindow<DialogueGraphWindow>(typeof(SceneView));
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable()
    {
        dialogueInspector = GetWindow<DialogueInspector>(typeof(InspectorElement));
        dialogueInspector.GetElements(graphView, this);

        ConstructGraph();
        GenerateToolbar();

        //Methods for detecting changes
        graphView.onChange += OnProjectChange;
        AssemblyReloadEvents.beforeAssemblyReload += CheckForUnsaved;
        AssemblyReloadEvents.afterAssemblyReload += ReloadCurrent;        
    }
    
    private void OnDisable()
    {
        graphView.onChange -= OnProjectChange;
        rootVisualElement.Remove(graphView);
    }

    public void ConstructGraph()
    {
        graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph"
        };

        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    public void CreateNew()
    {
        graphView.onChange -= OnProjectChange;
        rootVisualElement.Remove(graphView);

        graphView.Clear();

        ConstructGraph();
        GenerateToolbar();

        //Methods for detecting changes
        graphView.onChange += OnProjectChange;
    }

    #region - Graph Elements -
    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        toolbar.Add(new ToolbarSpacer());
        toolbar.Add(new ToolbarSpacer());

        toolbar.Add(new Button(() => SaveChanges()) { text = "Save" });
        toolbar.Add(new Button(() => OnLoad()) { text = "Load" });

        toolbar.Add(new ToolbarSpacer());

        toolbar.Add(new Button(() => ToggleMiniMap()) { text = "MiniMap" });

        rootVisualElement.Add(toolbar);
    }

    //I think I'd also like to add a toolbar button to toggle this
    private void GenerateMiniMap()
    {
        if (miniMap != null)
        {
            Debug.Log("Minimap already exists");
        }
        miniMap = new MiniMap { anchored = true };
        miniMap.SetPosition(new Rect(this.position.width - 210, 30, 200, 140));
        //var coords = graphView.contentViewContainer.WorldToLocal(new Vector2(this.position.x - 10, 30));
        //miniMap.SetPosition(new Rect(coords.x, coords.y, 200, 140));

        //Make resizable
        miniMap.capabilities |= Capabilities.Resizable;
        miniMap.Add(new ResizableElement());

        //This does nothing so far
        miniMap.capabilities |= Capabilities.Collapsible;

        graphView.Add(miniMap);
    }
    
    //Add or remove the mini map
    private void ToggleMiniMap()
    {
        if (miniMap == null) GenerateMiniMap();
        else if (!graphView.Contains(miniMap)) graphView.Add(miniMap);
        else graphView.Remove(miniMap);
    }
    #endregion

    //I don't fully know what this does yet
    private void OnGUI()
    {
        //Ensures minimap stays in the corner... of course this may cause some issues if it was re-size or moved
        if (Event.current.rawType == EventType.Layout)
        {
            miniMap?.SetPosition(new Rect(this.position.width - 210, 30, 200, 140));
        }


        saveChangesMessage = EditorGUILayout.TextField(saveChangesMessage);

        EditorGUILayout.LabelField(hasUnsavedChanges ? "I have changes!" : "No changes.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("Try to close the window.");

        using (new EditorGUI.DisabledScope(hasUnsavedChanges))
        {
            if (GUILayout.Button("Create unsaved changes"))
                hasUnsavedChanges = true;
        }

        using (new EditorGUI.DisabledScope(!hasUnsavedChanges))
        {
            if (GUILayout.Button("Save"))
                SaveChanges();

            if (GUILayout.Button("Discard"))
                DiscardChanges();
        }
    }

    #region - Saving/Loading -   
    private void OnLoad()
    {
        var saveUtility = GraphSaveUtility.GetInstance(graphView);
        saveUtility.LoadGraph(fileName);
    }

    //Attempts and fails to save changes prior to reloading
    private void CheckForUnsaved()
    {
        if (hasUnsavedChanges)
        {
            Debug.LogWarning("Assembly Reloads may cause data loss in unsaved Dialogue Graphs");
            var saveUtility = GraphSaveUtility.GetInstance(graphView);
            saveUtility.SaveGraph(fileName);
            hasUnsavedChanges = false;
        }
    }

    //Reload the current file name since it closes upon assembly
    private void ReloadCurrent()
    {
        OpenFile(fileName);
    }

    public void OnProjectChange()
    {
        //Will prompt the player if there are unsaved changes before closing 
        hasUnsavedChanges = true;
    }

    //Open a file by its name
    public void OpenFile(string fileName)
    {
        var saveUtility = GraphSaveUtility.GetInstance(graphView);
        saveUtility.LoadGraph(fileName);

        this.fileName = fileName;
    }

    //Call this when opening a new file, since there aren't actual changes
    public void OnFileOpened()
    {
        hasUnsavedChanges = false;
    }

    public override void SaveChanges()
    {
        if (string.IsNullOrEmpty(fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "OK");
            return;
        }

        hasUnsavedChanges = false;
        var saveUtility = GraphSaveUtility.GetInstance(graphView);
        saveUtility.SaveGraph(fileName);
        base.SaveChanges();
    }

    public override void DiscardChanges()
    {
        var saveUtility = GraphSaveUtility.GetInstance(graphView);
        saveUtility.ClearGraph();
        base.DiscardChanges();
    }
    #endregion
}
