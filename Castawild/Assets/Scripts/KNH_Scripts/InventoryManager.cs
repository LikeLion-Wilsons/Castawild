using System.Collections.Generic;
using UnityEngine;

public delegate void OnItemGet();

public class InventoryManager : MonoBehaviour
{
    public static event OnItemGet onItemGet;

    public static List<Item> slot_List = new List<Item>();

    // 아이템 획득
    public static void GetItem(Item_Scriptable scriptableData, int amount)
    {
        int id = scriptableData.itemID;

        // 이미 존재하는 아이템이면 개수만 증가
        for (int i = 0; i < slot_List.Count; i++)
        {
            if (slot_List[i].item_Data.itemID == id)
            {
                slot_List[i].count += amount;
                onItemGet?.Invoke();
                return;
            }
        }

        // 없으면 새로 추가
        Item newItem = new Item
        {
            item_Data = scriptableData,
            count = amount
        };

        slot_List.Add(newItem);
        onItemGet?.Invoke();
    }

    // 아이템 소지 여부 확인
    public static bool HaveItem(int id)
    {
        foreach (var item in slot_List)
        {
            if (item.item_Data.itemID == id)
                return true;
        }
        return false;
    }
}