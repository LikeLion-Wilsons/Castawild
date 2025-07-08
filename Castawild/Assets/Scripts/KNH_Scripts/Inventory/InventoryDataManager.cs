using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnItemGet();

public class InventoryDataManager : MonoBehaviour
{
    public static InventoryDataManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public static event Action<Item> onNewItemAdded;//새로운 아이템이 추가될 때
    public static event Action onInventoryUpdated;//기존에 있던 아이템이 추가될 때

    public List<Item> slot_List = new List<Item>();

    // 아이템 획득
    public  void GetItem(Item_Scriptable scriptableData, int amount)
    {
        int id = scriptableData.itemID;
        // 이미 존재하는 아이템이면 개수만 증가
        for (int i = 0; i < slot_List.Count; i++)
        {
            if (slot_List[i].item_Data.itemID == id)
            {
                slot_List[i].count += amount;
                onInventoryUpdated?.Invoke();
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
        onNewItemAdded?.Invoke(newItem);
    }

    //아이템 버리기
    public void ThrowItem(int id)
    {
        for (int i = 0; i < slot_List.Count; i++)
        {
            if (slot_List[i].item_Data.itemID == id)
            {
                slot_List.RemoveAt(i);
                return;
            }
        }
    }

    // 아이템 소지 여부 확인
    public bool HaveItem(int id)
    {
        foreach (var item in slot_List)
        {
            if (item.item_Data.itemID == id)
                return true;
        }
        return false;
    }

    public List<Item> GetItemList()
    {
        return slot_List;
    }

}