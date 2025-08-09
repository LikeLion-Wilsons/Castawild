using System.Collections.Generic;
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
    public List<Item_Scriptable> ingredient;//재료 아이템 
    public List<int> ingredientCount;//재료 아이템 수량
    public GameObject buildPreviewPrefab;
    public GameObject buildPrefab;
}
