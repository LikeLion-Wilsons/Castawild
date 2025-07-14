using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public delegate void OnItemGet();

public class InventoryDataManager : MonoBehaviour
{
    [SerializeField] int maxStackCount;//아이템 최대 스택 개수
    public List<Item> itemList;
    public Item_Panel[] inventorySlots;
    public GameObject inventoryItemPrefab;
    int selectedSlot = -1;
    int maxSlotCount = 9; // 총 슬롯 수
    public static InventoryDataManager Instance { get; private set; }
    private void Awake()
    {
        itemList = new List<Item>();
        for (int i = 0; i < 29; i++)
        {
            itemList.Add(new Item
            {
                item_Data = null,
                count = 0,
                durability = 1
            });
        }

        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);

    }
    void ChangeSelectedSlot(int newValue)
    {
        if (Canvas_Holder.instance.IsInventoryOpen()) return;
        if (selectedSlot >= 0)
        {
            inventorySlots[selectedSlot].Deselect();
        }
        inventorySlots[newValue].Select();
        selectedSlot = newValue;
    }
   

    public static event Action onInventoryUpdated;//기존에 있던 아이템이 추가될 때



    private void Start()
    {
        ChangeSelectedSlot(0);
    }

    private void Update()
    {
       if(Input.inputString != null)
        {
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if(isNumber && number > 0 && number < 10)
            {
                ChangeSelectedSlot(number - 1);
            }
        }
        // 마우스 휠 입력
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) // 휠 위로
        {
            int next = (selectedSlot - 1 + maxSlotCount) % maxSlotCount;
            ChangeSelectedSlot(next);
        }
        else if (scroll < 0f) // 휠 아래로
        {
            int next = (selectedSlot + 1) % maxSlotCount;
            ChangeSelectedSlot(next);
        }
    }
    public Item_Scriptable GetSeletedItem(bool use)
    {
        Item_Panel slot = inventorySlots[selectedSlot];
        if (slot.item != null)
        {
            Item_Scriptable item = slot.item.item_Data;
            if (use)
            {
                slot.item.count--;
                if(slot.item.count <= 0)
                {
                    itemList[selectedSlot] = null;
                }
                onInventoryUpdated?.Invoke();
            }
            return null;
        }
        return null;
    }

    // 아이템 획득
    public bool GetItem(Item_Scriptable scriptableData, int amount)
    {
        int id = scriptableData.itemID;
        // 이미 존재하는 아이템이면 개수만 증가
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].item_Data != null)
            {
                if (itemList[i].item_Data.type == Item_Type.Equipment) maxStackCount = 1;
                else maxStackCount = 20;
                if (itemList[i].item_Data.itemID == id && itemList[i].count < maxStackCount)
                {
                    itemList[i].count += amount;
                    onInventoryUpdated?.Invoke();
                    return true;
                }
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
                return true;
            }
        }
        return false;
    }

    //아이템 버리기
    public void ThrowItem(int index)
    {
        if (index >= 0 && index < itemList.Count)
        {
            Debug.Log(itemList[index].item_Data.name + " 버림!");
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

    public int GetSelectedIndex()
    {
        return selectedSlot;
    }
}