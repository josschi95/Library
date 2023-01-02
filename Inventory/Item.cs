using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Generic")]
public class Item : ScriptableObject
{
    [SerializeField] private string m_name;
    [SerializeField] private int m_IDNumber;
    [SerializeField] private GameObject m_prefab;
    [Space]
    [SerializeField] private bool m_canStack;
    [SerializeField] private int m_value;
    [SerializeField] private float m_weight;

    public string Name => m_name;
    public int IDNumber => m_IDNumber;
    public GameObject Prefab => m_prefab;
    public bool CanStack => m_canStack;
    public int Value => m_value;
    public float Weight => m_weight;

    public void Use()
    {
        //Meant to be overwritten
        Debug.Log("Using " + Name);
    }
}
