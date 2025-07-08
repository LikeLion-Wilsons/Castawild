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

    public static event Action onInventoryUpdated;//기존에 있던 아이템이 추가될 때

    public List<Item> itemList = new List<Item>();

    private void Start()
    {
        // 부족한 부분 null 채우기
        while (itemList.Count < 20)
        {
            itemList.Add(null);
        }
    }
    // 아이템 획득
    public  void GetItem(Item_Scriptable scriptableData, int amount)
    {
        int id = scriptableData.itemID;
        // 이미 존재하는 아이템이면 개수만 증가
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].item_Data == null) continue;
            if (itemList[i].item_Data.itemID == id)
            {
                itemList[i].count += amount;
                onInventoryUpdated?.Invoke();
                return;
            }
        }
        // 빈 슬롯 찾기
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].item_Data == null)
            {
                Item newItem = new Item { item_Data = scriptableData, count = amount };
                itemList[i] = newItem;
                onInventoryUpdated?.Invoke();
                return;
            }
        }
    }

    //아이템 버리기
    public void ThrowItem(int index)
    {
        if (index >= 0 && index < itemList.Count)
        {
            itemList[index] = null;
            onInventoryUpdated?.Invoke();
        }
    }

    // 아이템 소지 여부 확인
    public bool HaveItem(int id)
    {
        foreach (var item in itemList)
        {
            if (item != null && item.item_Data.itemID == id)
                return true;
        }
        return false;
    }

    public List<Item> GetItemList()
    {
        return itemList;
    }

}