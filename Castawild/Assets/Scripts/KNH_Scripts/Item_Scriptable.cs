using UnityEngine;

[CreateAssetMenu(fileName = "Item_Scriptable", menuName = "Scriptable Objects/Item_Scriptable")]
public class Item_Scriptable : ScriptableObject
{
    public int itemID;
    public string itemName;
    public string description;
    public Item_Type type;
}
