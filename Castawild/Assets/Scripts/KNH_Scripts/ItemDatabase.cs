using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataBase", menuName = "ScriptableObjects/ItemDataBase", order = 5)]
public class ItemDataBase : ScriptableObject
{
    public static ItemDataBase Instance => Resources.Load<ItemDataBase>("Scriptable/ItemDataBase");

    public List<Item_Scriptable> items;

    public Item_Scriptable GetItemByID(int id)
    {
        return items.Find(item => item.itemID == id);
    }
}
