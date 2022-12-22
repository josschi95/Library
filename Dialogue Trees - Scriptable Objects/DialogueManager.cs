using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds and manages all conditions which may be referenced during dialogue
/// Holds all callbacks which will be called to run dialogue
/// </summary>

public class DialogueManager : MonoBehaviour
{
    #region - Singleton -
    public static DialogueManager instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    private Dictionary<string, bool> DialogueBooleans;
    private Dictionary<string, int> DialogueIntegers;

    [SerializeField] private DialogueBool[] dialogueBools;
    [SerializeField] private DialogueInt[] dialogueInts;

    private void Start()
    {
        ConstructDictionaries();
    }

    //Convert all variable arrays set in inspector into dictionaries
    public void ConstructDictionaries()
    {
        DialogueBooleans = new Dictionary<string, bool>();
        DialogueIntegers = new Dictionary<string, int>();

        //Converts bools set in editor to dictionary
        for (int i = 0; i < dialogueBools.Length; i++)
        {
            if (dialogueBools[i].key != "") //Ignore any empty fields
                DialogueBooleans.Add(dialogueBools[i].key, dialogueBools[i].value);
        }

        //Converts integers set in editor to dictionary
        for (int i = 0; i < dialogueInts.Length; i++)
        {
            if (dialogueInts[i].key != "") //Ignore any empty fields
                DialogueIntegers.Add(dialogueInts[i].key, dialogueInts[i].value);
        }
    }

    #region - Dialogue Variables -
    public static bool GetBool(string key)
    {
        if (instance.DialogueBooleans.ContainsKey(key))
            return instance.DialogueBooleans[key];

        else throw new DialogueException("Key does not exist.");
    }

    public static void SetBool(string key, bool value)
    {
        if (instance.DialogueBooleans.ContainsKey(key))
        {
            instance.DialogueBooleans[key] = value;
            Debug.Log(key + " set to " + value);
        }
        else throw new DialogueException("Key does not exist.");
    }

    public static int GetInt(string key)
    {
        if (instance.DialogueIntegers.ContainsKey(key))
            return instance.DialogueIntegers[key];

        else throw new DialogueException("Key does not exist.");
    }

    public static void SetInt(string key, int value)
    {
        if (instance.DialogueIntegers.ContainsKey(key))
        {
            instance.DialogueIntegers[key] = value;
            Debug.Log(key + " set to " + value);
        }
        else throw new DialogueException("Key does not exist.");
    }
    #endregion

    #region - Callbacks -
    public delegate void DialogueCallback(Dialogue dialogue);
    public DialogueCallback onDialogueRequested;
    public DialogueCallback onDialogueStart;
    public DialogueCallback onDialogueEnd;

    public delegate void DialogueNodeCallback(DialogueNode node);
    public DialogueNodeCallback onDialogueNodeRequested;
    public DialogueNodeCallback onDialogueNodeStart;
    public DialogueNodeCallback onDialogueNodeEnd;

    public static void RaiseRequestDialogue(Dialogue dialogue)
    {
        instance.onDialogueRequested?.Invoke(dialogue);
    }

    public static void RaiseDialogueStart(Dialogue dialogue)
    {
        instance.onDialogueStart?.Invoke(dialogue);
    }

    public static void RaiseDialogueEnd(Dialogue dialogue)
    {
        instance.onDialogueEnd?.Invoke(dialogue);
    }


    public static void RaiseRequestDialogueNode(DialogueNode node)
    {
        instance.onDialogueNodeRequested?.Invoke(node);
    }

    public static void RaiseDialogueNodeStart(DialogueNode node)
    {
        instance.onDialogueNodeStart?.Invoke(node);
    }

    public static void RaiseDialogueNodeEnd(DialogueNode node)
    {
        instance.onDialogueNodeEnd?.Invoke(node);
    }
    #endregion
}

[System.Serializable]
public class DialogueBool
{
    public string key;
    public bool value;
}

[System.Serializable]
public class DialogueInt
{
    public string key;
    public int value;
}

[System.Serializable]
public class DialogueCondition
{
    public string keyName;
    public VariableType type;

    [Tooltip("Only relevant if type == Bool. Condition is met if key value is same")]
    public BooleanValue boolValue;

    [Header("Integers")]
    public IntegerValue integerComparison;
    public int comparedValue;

    public bool ConditionMet()
    {
        switch (type)
        {
            case VariableType.Bool:
                //Returns true if both are true or both are false
                if (boolValue == BooleanValue.True && DialogueManager.GetBool(keyName)) return true;
                if (boolValue == BooleanValue.False && !DialogueManager.GetBool(keyName)) return true;
                return false;
            case VariableType.Int:
                int i = DialogueManager.GetInt(keyName);
                switch (integerComparison)
                {
                    case IntegerValue.Less:
                        if (i < comparedValue) return true;
                        break;
                    case IntegerValue.LessOrEqual:
                        if (i <= comparedValue) return true;
                        break;
                    case IntegerValue.Equal:
                        if (i == comparedValue) return true;
                        break;
                    case IntegerValue.GreaterOrEqual:
                        if (i >= comparedValue) return true;
                        break;
                    case IntegerValue.Greater:
                        if (i > comparedValue) return true;
                        break;
                }
                break;
        }
        return false;
    }
}

[System.Serializable]
public class DialogueAffector
{
    public string keyName;
    public VariableType type;
    [Space]
    [Tooltip("Only relevant if type == Bool. Sets variable to value")]
    public BooleanValue boolValue;
    public IntegerModifier integerEffect;
    public int value;

    public void OnPull()
    {
        switch (type)
        {
            case VariableType.Bool:
                bool b = true;
                if (boolValue == BooleanValue.False) b = false;
                DialogueManager.SetBool(keyName, b);
                break;
            case VariableType.Int:
                switch (integerEffect)
                {
                    case IntegerModifier.SetTo:
                        DialogueManager.SetInt(keyName, value);
                        break;
                    case IntegerModifier.Add:
                        int sum = DialogueManager.GetInt(keyName) + value;
                        DialogueManager.SetInt(keyName, sum);
                        break;
                }
                break;
        }
    }
}

public enum VariableType { Bool, Int }
public enum BooleanValue { False, True }
public enum IntegerValue { Less, LessOrEqual, Equal, GreaterOrEqual, Greater }
public enum IntegerModifier { SetTo, Add }