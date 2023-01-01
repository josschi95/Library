using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat
{
    public delegate void OnStatValueChanged(Stat stat);
    public OnStatValueChanged onStatValueChanged;

    public int baseValue;
    [Tooltip("int +/- modifier to a stat")]
    public List<int> modifiers = new List<int>();


    public void SetBaseValue(int value)
    {
        baseValue = value;

        if (onStatValueChanged != null)
            onStatValueChanged.Invoke(this);
    }

    //Get net value with all modifiers
    public int GetValue()
    {
        int finalValue = baseValue;
        for (int i = 0; i < modifiers.Count; i++)
        {
            finalValue += modifiers[i];
        }

        return finalValue;
    }

    //Add a temporary modifier
    public void AddModifier(int value)
    {
        if (value != 0) flatModifiers.Add(value);

        if (onStatValueChanged != null)
            onStatValueChanged.Invoke(this);
    }

    //Remove a temporary modifier
    public void RemoveModifier(int value)
    {
        if (value != 0) modifiers.Remove(value);

        if (onStatValueChanged != null)
            onStatValueChanged.Invoke(this);
    }
}

public enum Stats { HP, MP, STR, DEF, MAG, RES, SPD, DEX }
