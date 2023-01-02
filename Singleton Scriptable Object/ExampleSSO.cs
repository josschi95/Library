using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExampleSSO", menuName = "Scriptable Objects/Example SSO")]
public class ExampleSSO : SingletonScriptableObject<ExampleSSO>
{
    public delegate void OnSettingsChanged();
    public OnSettingsChanged onSettingsChanged;

    [SerializeField] private string m_gameTitle;
    [SerializeField] private string m_gameVersion;
    public string GameTitle => m_gameTitle;
    public string GameVersion => m_gameVersion;

    public static void UpdateVersion(string newTitle)
    {
        Instance.m_gameVersion = newTitle;
        Instance.onSettingsChanged?.Invoke();
    }
}
