using UnityEngine;

[CreateAssetMenu(fileName = "Item_Scriptable", menuName = "ScriptableObjects/ItemData", order = 4)]
public class Item_Scriptable : ScriptableObject
{
    public int itemID;
    public string itemName;
    public string description;
    public Item_Type type;
}
