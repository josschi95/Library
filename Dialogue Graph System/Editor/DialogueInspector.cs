using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEditor.Experimental.GraphView;
using System.Linq;

public class DialogueInspector : EditorWindow
{
    private delegate void OnVariableChangedCallback();
    private OnVariableChangedCallback onVariableChanged;

    private DialogueGraphView graphView;
    private DialogueGraphWindow dialogueGraph;

    private string header = "Dialogue Inspector";
    private string variableCacheName = "_DialogueVariables";

    #region - Selected Node Properties -
    private DialogueNode currentNode;
    private string nodeGuid = "";
    private ReorderableList nodeCondtitions;

    private string[] comparisonOptions = { "==", "!=" }; //for text and bools
    private string[] numberComparisonOptions = { "==", "!=", "<", "<=", ">", ">=" }; //for numbers
    #endregion

    #region - Variables -
    private VariableContainer variableCache;
    private ReorderableList variablesList;
    public List<DialogueVariable> variables = new List<DialogueVariable>();

    private string[] variableOptions = { "Text", "Number", "Bool" };
    private string[] boolOptions = { "False", "True" };
    private List<string> variableStrings = new List<string>();
    #endregion

    private DialogueContainer currentNarrative;
    private List<DialogueContainer> storedFiles;
    private List<string> fileNames = new List<string>();

    [MenuItem("Dialogue/Dialogue Inspector")]
    public static void OpenDialogueNodeWindow()
    {
        var window = GetWindow<DialogueInspector>(typeof(InspectorElement));
        window.titleContent = new GUIContent("Dialogue");
    }

    private void OnEnable()
    {
        titleContent = new GUIContent("Dialogue");
        LoadSavedVariables();
        CreateReorderableList();
        GetNarratives();

        onVariableChanged += OnVariablesChanged;
    }

    private void OnDisable()
    {
        onVariableChanged -= OnVariablesChanged;
    }

    public void GetElements(DialogueGraphView graphView, DialogueGraphWindow dialogueGraph)
    {
        this.graphView = graphView;
        this.dialogueGraph = dialogueGraph;
    }

    private void CreateReorderableList()
    {       
        //Variables ReorderableList
        variablesList = new ReorderableList(variables, typeof(string), true, true, true, true);
        variablesList.drawHeaderCallback = VariablesDrawHeader;
        variablesList.drawElementCallback = VariablesDrawListItems;
        variablesList.onAddCallback += OnAddVariable;
        variablesList.onChangedCallback += delegate { OnVariablesChanged(); };
    }

    #region - Node Selection -
    public static void OnNodeSelected(DialogueNode node)
    {
        GetWindow<DialogueInspector>().DisplayNodeProperties(node);
    }

    public void DisplayNodeProperties(DialogueNode node)
    {
        //Don't allow the editing of the entry point node
        if (node.entryPoint) return;

        currentNode = node;
        nodeGuid = currentNode.GUID;
        CheckForMissingVariables();
    }

    public static void OnNodeUnselected()
    {
        GetWindow<DialogueInspector>().ClearNode();
    }

    private void ClearNode()
    {
        currentNode = null;
    }
    #endregion

    private void OnGUI()
    {
        //Header
        GUILayout.Label(header, EditorStyles.boldLabel);

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save Narrative")) SaveCurrentNarrative();
        if (GUILayout.Button("New Narrative")) CreateNewNarrative();
        
        EditorGUI.BeginChangeCheck();
        int i = storedFiles.IndexOf(currentNarrative);
        i = EditorGUILayout.Popup("     Open Narrative", i, fileNames.ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            //Open selected narrative
            SaveCurrentNarrative();
            dialogueGraph.OpenFile(fileNames[i]);
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        //Dialogue Variables List
        GUILayout.BeginHorizontal();
        GUILayout.Label("Variables", EditorStyles.boldLabel);
        if (GUILayout.Button("Save Variables")) SaveCurrentVariables();
        GUILayout.EndHorizontal();
        variablesList.DoLayoutList();

        //Current Narrative
        GUILayout.Label("Current Narrative");
        GUILayout.BeginHorizontal();
        string fileName = "";
        if (currentNarrative != null) fileName = currentNarrative.name;
        EditorGUI.BeginChangeCheck();
        fileName = GUILayout.TextField(fileName);
        if (EditorGUI.EndChangeCheck())
        {
            dialogueGraph.fileName = fileName;
        }
        
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        if (currentNode != null) DisplayCurrentNode();
    }

    //Adds GUI components to edit the currently selected Node
    private void DisplayCurrentNode()
    {
        //Header & GUID
        GUILayout.BeginHorizontal();
        GUILayout.Label("Current Node", EditorStyles.boldLabel);
        GUILayout.Label(nodeGuid, EditorStyles.largeLabel);
        GUILayout.EndHorizontal();

        //Toggle - isPlayerNode
        bool b = currentNode.isPlayerNode;
        EditorGUI.BeginChangeCheck();
        b = EditorGUILayout.Toggle("Is Player Node", b);
        if (EditorGUI.EndChangeCheck() && b != currentNode.isPlayerNode) ToggleNodeIsPlayer(b);

        //Subheader
        GUILayout.Label("Dialogue", EditorStyles.largeLabel);
        string dialogueText = currentNode.dialogueText;
        EditorGUI.BeginChangeCheck();
        dialogueText = GUILayout.TextArea(currentNode.dialogueText, GUILayout.Height(75));
        if (EditorGUI.EndChangeCheck()) UpdateNodeText(dialogueText);

        GUILayout.Space(10);

        //Conditions
        nodeCondtitions = new ReorderableList(currentNode.conditions, typeof(string), true, true, true, true);
        nodeCondtitions.drawHeaderCallback = ConditionsDrawHeader;
        nodeCondtitions.drawElementCallback = ConditionsDrawListItems;
        nodeCondtitions.onAddCallback += AddNewNodeCondition;
        nodeCondtitions.DoLayoutList();

        //Events
        //another reorderable lsit
        //probably a base class of NodeEvent
        //deriving from that would be DialogueEvents... and probably scriptableObject events
    }

    #region - Variables List -
    private void VariablesDrawHeader(Rect rect)
    {
        var win = rect.width;
        float leftIndent = rect.x + 20;
        var w1 = win * 0.35f; var w2 = win * 0.15f; var w3 = win * 0.35f;
        var rect1 = new Rect(leftIndent, rect.y, w1, rect.height);
        var rect2 = new Rect(leftIndent + w1 + 10, rect.y, w2, rect.height);
        var rect3 = new Rect(leftIndent + w1 + w2 + 30, rect.y, w3, rect.height);
        
        GUILayout.BeginHorizontal();

        EditorGUI.LabelField(rect1, "Name");
        EditorGUI.LabelField(rect2, "Type");
        EditorGUI.LabelField(rect3, "Value");

        GUILayout.EndHorizontal();
    }

    void VariablesDrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        var win = rect.width;
        var w1 = win * 0.35f; var w2 = win * 0.25f; var w3 = win * 0.35f;
        var rect1 = new Rect(rect.x, rect.y, w1, rect.height);
        var rect2 = new Rect(rect.x + w1 + 10, rect.y, w2, rect.height);
        var rect3 = new Rect(rect.x + w1 + w2 + 20f, rect.y, w3, rect.height);

        GUILayout.BeginHorizontal();

        //Variable Name
        string newName = variables[index].name;
        EditorGUI.BeginChangeCheck();
        newName = EditorGUI.TextField(rect1, newName);
        if (EditorGUI.EndChangeCheck())
        {
            bool duplicateName = false;
            for (int v = 0; v < variables.Count; v++)
            {
                if (variables[v].name == newName)
                {
                    EditorUtility.DisplayDialog("Duplicate Variable Name", "Please select a unique name for your variable", "OK");
                    duplicateName = true;
                    break;
                }
            }
            if (!duplicateName) variables[index].name = newName;
            variables[index].onVariableChanged?.Invoke();
            onVariableChanged?.Invoke();
        }

        //Variable Type
        EditorGUI.LabelField(rect2, variables[index].type.ToString());
        int i = (int)variables[index].type;
        EditorGUI.BeginChangeCheck();
        i = EditorGUI.Popup(rect2, i, variableOptions);
        if (EditorGUI.EndChangeCheck())
        {
            variables[index].SetVaraibleType((VariableType)i);
            variables[index].onVariableChanged?.Invoke();
            onVariableChanged?.Invoke();
        }
        
        //Variable Value
        switch (variables[index].type)
        {
            case VariableType.Text:
                EditorGUI.BeginChangeCheck();
                variables[index].variableValue = EditorGUI.TextField(rect3, variables[index].variableValue);
                if (EditorGUI.EndChangeCheck()) onVariableChanged?.Invoke();
                break;
            case VariableType.Number:
                EditorGUI.BeginChangeCheck();
                string currentValue = variables[index].variableValue;
                currentValue = EditorGUI.TextField(rect3, variables[index].variableValue);
                if (EditorGUI.EndChangeCheck())
                {
                    //The variable type is a number, check to make sure a valid number was input
                    float newValue;
                    if (float.TryParse(currentValue, out newValue))
                    {
                        variables[index].SetValue(newValue);
                        variables[index].onVariableChanged?.Invoke();
                        onVariableChanged?.Invoke();
                    }
                }
                break;
            case VariableType.Bool:
                EditorGUI.LabelField(rect3, variables[index].GetBoolValue().ToString());
                int x = 0; if (variables[index].GetBoolValue()) x = 1;
                EditorGUI.BeginChangeCheck();
                x = EditorGUI.Popup(rect3, x, boolOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    bool b = false; if (x == 1) b = true;
                    variables[index].SetValue(b);
                    variables[index].onVariableChanged?.Invoke();
                    onVariableChanged?.Invoke();
                }
                break;
        }

        GUILayout.EndHorizontal();
    }

    private void OnAddVariable(ReorderableList list)
    {
        var newVariable = new DialogueVariable();
        for (int i = 0; i < variables.Count; i++)
        {
            if (variables[i].name == newVariable.name)
            {
                newVariable.name += "(1)";
            }
        }
        variables.Add(newVariable);
        onVariableChanged?.Invoke();
    }

    //Call this method whenever a variable is changed to update strings list and repaint
    private void OnVariablesChanged()
    {
        if (currentNode != null) CheckForMissingVariables();

        variableStrings.Clear();
        for (int i = 0; i < variables.Count; i++)
        {
            variableStrings.Add(variables[i].name);
        }
        Repaint();
    }
    #endregion

    #region - Node Editing
    //Change whether the node is a player node or not
    private void ToggleNodeIsPlayer(bool isPlayer)
    {
        if (currentNode == null) return;

        currentNode.isPlayerNode = isPlayer;

        if (currentNode.isPlayerNode)
        {
            currentNode.styleSheets.Remove(Resources.Load<StyleSheet>("Node_NPC"));
            currentNode.styleSheets.Add(Resources.Load<StyleSheet>("Node_Player"));
        }
        else
        {
            currentNode.styleSheets.Remove(Resources.Load<StyleSheet>("Node_Player"));
            currentNode.styleSheets.Add(Resources.Load<StyleSheet>("Node_NPC"));
        }
    }

    //Change the dialogue of the node
    private void UpdateNodeText(string newDialogue)
    {
        if (currentNode == null) return;

        currentNode.dialogueText = newDialogue;
        currentNode.dialogueTextField.SetValueWithoutNotify(newDialogue);
        Debug.Log("Updated Node Text");
    }

    //Draw Node Conditions Header
    private void ConditionsDrawHeader(Rect rect)
    {
        var win = rect.width;
        float leftIndent = rect.x + 20;
        var w1 = win * 0.35f; var w2 = win * 0.15f; var w3 = win * 0.35f;
        var rect1 = new Rect(rect.x, rect.y, w1, rect.height);
        //var rect2 = new Rect(leftIndent + w1 + 10, rect.y, w2, rect.height);
        //var rect3 = new Rect(leftIndent + w1 + w2 + 30, rect.y, w3, rect.height);

        //GUILayout.BeginHorizontal();

        EditorGUI.LabelField(rect1, "Conditions");
        //EditorGUI.LabelField(rect2, "Must be");
        //EditorGUI.LabelField(rect3, "Value");

        //GUILayout.EndHorizontal();
    }

    //Draw Node Conditions List
    private void ConditionsDrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        var condition = currentNode.conditions[index];
        if (condition.pairedVariable == null)
        {
            Debug.LogWarning("Missing Variable");
            currentNode.conditions.RemoveAt(index);
            return;
        }

        var win = rect.width;
        var w1 = win * 0.35f; var w2 = win * 0.25f; var w3 = win * 0.35f; var space = win * 0.05f;
        var rect1 = new Rect(rect.x, rect.y, w1, rect.height);
        var rect2 = new Rect(rect.x + w1, rect.y, w2, rect.height);
        var rect3 = new Rect(rect.x + w1 + w2, rect.y, w3, rect.height);

        GUILayout.BeginHorizontal();

        //Name of the variable which it will be compared against
        //Popup for the available variables which can be assigned as conditions
        int currentVariableIndex = variables.IndexOf(condition.pairedVariable);
        EditorGUI.LabelField(rect1, condition.name); //Label for name of current condition/variable

        EditorGUI.BeginChangeCheck();
        currentVariableIndex = EditorGUI.Popup(rect1, currentVariableIndex, variableStrings.ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            //The variable condition has changed, reset the condition in the node to its default value of the new type
            condition.SetPairedVariable(variables[currentVariableIndex]);
        }

        //Comparison Type
        EditorGUI.LabelField(rect2, condition.comparisonType.ToString()); //Label
        int compareType = (int)condition.comparisonType;
            EditorGUI.BeginChangeCheck();
        if (condition.type == VariableType.Number)
            compareType = EditorGUI.Popup(rect2, compareType, numberComparisonOptions);
        else compareType = EditorGUI.Popup(rect2, compareType, comparisonOptions);
        if (EditorGUI.EndChangeCheck())
        {
            condition.SetComparisonType((ComparisonType)compareType);
        }

        //Variable Value
        switch (condition.type)
        {
            case VariableType.Text:
                condition.variableValue = EditorGUI.TextField(rect3, condition.variableValue);
                break;
            case VariableType.Number:
                EditorGUI.BeginChangeCheck();
                string currentValue = condition.variableValue;
                currentValue = EditorGUI.TextField(rect3, currentValue);
                if (EditorGUI.EndChangeCheck())
                {
                    //The variable type is a number, check to make sure a valid number was input
                    float newValue;
                    if (float.TryParse(currentValue, out newValue)) condition.SetValue(newValue);
                }
                break;
            case VariableType.Bool:
                EditorGUI.LabelField(rect3, condition.GetBoolValue().ToString());
                int x = 0; if (condition.GetBoolValue()) x = 1;
                EditorGUI.BeginChangeCheck();
                x = EditorGUI.Popup(rect3, x, boolOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    bool b = false; if (x == 1) b = true;
                    condition.SetValue(b);
                }
                break;
        }

        GUILayout.EndHorizontal();
    }

    //Add a new condition to the current node
    private void AddNewNodeCondition(ReorderableList list)
    {
        if (currentNode == null) return;

        if (variables.Count == 0)
        {
            EditorUtility.DisplayDialog("No Variables Found", "Please add variables to create node conditions.", "Ok");
            return;
        }

        var newCondition = new DialogueVariable();
        newCondition.SetPairedVariable(variables[0]);
        currentNode.conditions.Add(newCondition);
    }

    //Check the current node for any variables which have been removed
    private void CheckForMissingVariables()
    {
        //Check to see if a variable was removed, and removed that condition if it was
        for (int i = currentNode.conditions.Count - 1; i >= 0; i--)
        {
            if (!variables.Contains(currentNode.conditions[i].pairedVariable))
            {
                if (FoundPairByName(i)) continue;
                else currentNode.conditions.RemoveAt(i);
            }
        }
    }

    //Check to see if there is a current variable with a matching name
    private bool FoundPairByName(int index)
    {
        //Connection may have been lost, check by name
        for (int i = 0; i < variables.Count; i++)
        {
            if (variables[i].name == currentNode.conditions[index].name && variables[i].type == currentNode.conditions[index].type)
            {
                //Most likely a match, set it to the found one add return;
                currentNode.conditions[index].SetPairedVariable(variables[i]);
                return true;
            }
        }
        return false;
    }
    #endregion

    #region - Saving -
    private void SaveCurrentVariables()
    {
        var variableContainer = ScriptableObject.CreateInstance<VariableContainer>();
        variableContainer.variables.Clear();
        variableContainer.variables.AddRange(variables);
        //Auto creates resources foldier if it does not exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        AssetDatabase.CreateAsset(variableContainer, $"Assets/Resources/{variableCacheName}.asset");
        AssetDatabase.SaveAssets();
    }

    private void LoadSavedVariables()
    {
        variableCache = Resources.Load<VariableContainer>(variableCacheName);
        if (variableCache == null)
        {
            EditorUtility.DisplayDialog("Existing Variable Files Not Found", "A new variable container will be created.", "OK");
            SaveCurrentVariables();
        }
    }

    private void SaveCurrentNarrative()
    {
        dialogueGraph.SaveChanges();
    }

    private void CreateNewNarrative()
    {
        //Save current narrative if there is one
        if (currentNarrative != null) SaveCurrentNarrative();

        dialogueGraph.CreateNew();

        //Create a new dialogueContainer
        var newNarrative = ScriptableObject.CreateInstance<DialogueContainer>();
        newNarrative.name = "New Narrative";

        //Set as current
        currentNarrative = newNarrative;

        GetWindow<DialogueGraphWindow>().ConstructGraph();
    }

    private void GetNarratives()
    {
        storedFiles = new List<DialogueContainer>();
        fileNames = new List<string>();

        string[] guids = AssetDatabase.FindAssets("t:" + typeof(DialogueContainer).Name);
        DialogueContainer[] containers = new DialogueContainer[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            containers[i] = AssetDatabase.LoadAssetAtPath<DialogueContainer>(path);
            fileNames.Add(containers[i].name);
        }
        storedFiles.AddRange(containers);
    }

    public void OnFileOpened(DialogueContainer container)
    {
        currentNarrative = container;
        GetNarratives();
    }
    #endregion
}
