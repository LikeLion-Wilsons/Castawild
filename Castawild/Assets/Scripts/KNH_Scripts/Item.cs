using Fusion;
using UnityEngine;

[System.Serializable]
public struct Item : INetworkStruct
{
    //public Item_Scriptable item_Data;
    public int itemID;
    public int count;
    public float durability;//내구도
    public bool isNull;

    public Item_Scriptable GetData()
    {
        return ItemDataBase.Instance.GetItemByID(itemID);
    }
}
