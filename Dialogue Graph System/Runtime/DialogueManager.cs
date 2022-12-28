using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("ERROR: More than one instance of DialogueManager found");
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    private VariableContainer variableCache;

    [SerializeField] private DialogueContainer dialogue;
    private List<DialogueVariable> variables = new List<DialogueVariable>();
    private Dictionary<string, DialogueVariable> variableDictionary;
    
    [ReadOnly] public string testString = "My Test String";


    private void Start()
    {
        LoadVariables();

        if (dialogue != null)
        {
            for (int i = 0; i < dialogue.dialogueNodeData.Count; i++)
            {
                Debug.Log(i + ": " + dialogue.dialogueNodeData[i].dialogueText);
            }
        }
    }

    private void LoadVariables()
    {
        variableCache = Resources.Load<VariableContainer>("_DialogueVariables");
        variables.AddRange(variableCache.variables);

        variableDictionary = new Dictionary<string, DialogueVariable>();
        for (int i = 0; i < variables.Count; i++)
        {
            variableDictionary.Add(variables[i].name, variables[i]);
        }
    }

    //Call this method to save all modified variables
    public void SaveVariables()
    {
        var container = ScriptableObject.CreateInstance<VariableContainer>();
        container.variables.Clear();
        container.variables.AddRange(variables);

        //Auto creates resources foldier if it does not exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        AssetDatabase.CreateAsset(container, "Assets/Resources/_DialogueVariables.asset");
        AssetDatabase.SaveAssets();
    }

    public static bool GetBoolVariableValue(string key)
    {
        return instance.variableDictionary[key].GetBoolValue();
    }

    public static float GetNumberVariableValue(string key)
    {
        return instance.variableDictionary[key].GetNumberValue();
    }

    public static string GetTextVariableValue(string key)
    {
        return instance.variableDictionary[key].GetStringValue();
    }
}
