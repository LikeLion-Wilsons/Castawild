using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnItemGet();
public class InventoryManager : MonoBehaviour
{
    public static event OnItemGet onItemGet;
    public static Dictionary<int, Item> item_List = new Dictionary<int, Item>();

    //아이템 획득
    public static void GetItem(Item_Scriptable scriptableData, int value)
    {
        Item item = new Item();
        item.item_Data = scriptableData;
        item.count = value;

        int id = item.item_Data.itemID;
        //아이템을 이미 가지고 있다면 개수만 증가
        if (HaveItem(id))
        {
            item_List[id].count += value;
        }
        //아이템 새로 추가
        else
        {
            item_List.Add(id, item);
        }
        onItemGet?.Invoke();
    }
    //아이템 소지 확인
    public static bool HaveItem(int value)
    {
        if (item_List.ContainsKey(value))
        {
            return true;
        }
        return false;
    }
}
