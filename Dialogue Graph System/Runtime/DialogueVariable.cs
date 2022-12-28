using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class DialogueVariable
{
    public delegate void OnVariableChangedCallback();
    public OnVariableChangedCallback onVariableChanged;

    public string name;
    public VariableType type;
    //If this is a variable cached by the DialogueManager, this is the runtime value of the variable
    //If this is a variable created as a condition for a node, it is the value that the runtime variable will be compared against
    public string variableValue;

    //Condition only properties
    public DialogueVariable pairedVariable; //store the variable which this condition will be compared to
    public ComparisonType comparisonType; //Only used by nodes, to decide how it will be compared to the runtime value

    public DialogueVariable(string name = "New Variable", VariableType type = VariableType.Text, string value = "", ComparisonType compareType = ComparisonType.Equal)
    {
        name = "New Variable";
        type = VariableType.Text;
        SetValue(value);
        comparisonType = compareType;
    }

    public void SetVaraibleType(VariableType type)
    {
        this.type = type;
        SetInitialValue();
    }

    public void SetComparisonType(ComparisonType type)
    {
        comparisonType = type;
    }

    //Only used for node conditions. Pass a stored variable to match values
    public void SetPairedVariable(DialogueVariable newPairedVariable)
    {
        if (pairedVariable != null)
        {
            pairedVariable.onVariableChanged -= OnPairedVariableChanged;
        }

        pairedVariable = newPairedVariable;
        pairedVariable.onVariableChanged += OnPairedVariableChanged;
        OnPairedVariableChanged();
    }

    //Condition only, call to update to changes in the paired variable
    private void OnPairedVariableChanged()
    {
        //Update name to match
        name = pairedVariable.name;
        //Update type to match
        if (type != pairedVariable.type) 
            SetVaraibleType(pairedVariable.type);
    }

    public void SetInitialValue()
    {
        switch (type)
        {
            case VariableType.Bool:
                variableValue = false.ToString();
                break;
            case VariableType.Number:
                variableValue = 0.ToString();
                break;
            case VariableType.Text:
                variableValue = "";
                break;
        }
        comparisonType = ComparisonType.Equal;
    }
    
    public bool ConditionMet(string value)
    {
        //Text and Bools can only be compared via == or !=
        if (type == VariableType.Text || type == VariableType.Bool)
        {
            if (comparisonType == ComparisonType.Equal && variableValue == value) return true;
            else if (comparisonType == ComparisonType.NotEqual && variableValue != value) return true;
            return false;
        }
        //Numbers
        float currentValue;
        float comparedToValue;
        if (float.TryParse(variableValue, out comparedToValue) && float.TryParse(value, out currentValue))
        {
            switch (comparisonType)
            {
                case ComparisonType.Equal:
                    return currentValue == comparedToValue;
                case ComparisonType.NotEqual:
                    return currentValue != comparedToValue;
                case ComparisonType.Less:
                    return currentValue < comparedToValue;
                case ComparisonType.LessOrEqual:
                    return currentValue <= comparedToValue;
                case ComparisonType.Greater:
                    return currentValue > comparedToValue;
                case ComparisonType.GreaterOrEqual:
                    return currentValue >= comparedToValue;
            }
        }
        throw new System.Exception("Attempting to compare one or more non-numbers as floats");
    }

    public void SetValue(string newValue)
    {
        variableValue = newValue;
    }

    public void SetValue(float newValue)
    {
        variableValue = newValue.ToString();
    }

    public void SetValue(bool newValue)
    {
        variableValue = newValue.ToString();
    }

    public string GetStringValue()
    {
        return variableValue;
    }

    public float GetNumberValue()
    {
        return float.Parse(variableValue);
    }

    public bool GetBoolValue()
    {
        if (variableValue == true.ToString()) return true;
        return false;
    }
}

public enum VariableType { Text, Number, Bool }
public enum ComparisonType { Equal, NotEqual, Less, LessOrEqual, Greater, GreaterOrEqual}