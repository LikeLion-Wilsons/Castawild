using Fusion;
using UnityEngine;

[System.Serializable]
public struct Item : INetworkStruct
{
    public int itemID;
    public int count;
    public float durability;//내구도

    public Item_Scriptable GetData()
    {
        return ItemDataBase.Instance.GetItemByID(itemID);
    }
}
