using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void OnItemGet();

public class InventoryDataManager : MonoBehaviour
{
    public static InventoryDataManager Instance { get; private set; }
    [SerializeField] int maxStackCount;//아이템 최대 스택 개수
    public List<Item> itemList = new List<Item>(29);
    public Item_Panel[] inventorySlots;
    public GameObject inventoryItemPrefab;
    int selectedSlot = -1;
    int maxSlotCount = 9; // 총 슬롯 수

    void ChangeSelectedSlot(int newValue)
    {
        if (selectedSlot >= 0)
        {
            inventorySlots[selectedSlot].Deselect();
        }
        inventorySlots[newValue].Select();
        selectedSlot = newValue;
    }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        for (int i = 0; i < 29; i++)
        {
            itemList.Add(null);
        }
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

    public bool AddItem(Item_Scriptable item)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            Item_Panel slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.item == item && itemInSlot.count < maxStackCount
                && itemInSlot.item.stackable)
            {
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return true;
            }
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            Item_Panel slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null)
            {
                SpawnNewItem(item, slot);
                return true;
            }
        }
        return false;
    }

    void SpawnNewItem(Item_Scriptable item, Item_Panel slot)
    {
        GameObject newItemGO = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGO.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item);
    }

    public Item_Scriptable GetSeletedItem(bool use)
    {
        Item_Panel slot = inventorySlots[selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if(itemInSlot != null)
        {
            Item_Scriptable item = itemInSlot.item;
            if (use)
            {
                itemInSlot.count--;
                if(itemInSlot.count <= 0)
                {
                    Destroy(itemInSlot.gameObject);
                }
                else
                {
                    itemInSlot.RefreshCount();
                }
            }
            return itemInSlot.item;
        }
        return null;
    }

    // 아이템 획득
    public void GetItem(Item_Scriptable scriptableData, int amount)
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
                    return;
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