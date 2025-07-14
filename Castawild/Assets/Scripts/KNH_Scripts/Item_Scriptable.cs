using UnityEngine;

[CreateAssetMenu(fileName = "Item_Scriptable", menuName = "ScriptableObjects/ItemData", order = 4)]
public class Item_Scriptable : ScriptableObject
{
    public int itemID;
    public Sprite image;
    public string itemName;
    public string description;
    public Item_Type type;
    public Vector2Int range = new Vector2Int(5, 4);
    public bool stackable = true;
}
