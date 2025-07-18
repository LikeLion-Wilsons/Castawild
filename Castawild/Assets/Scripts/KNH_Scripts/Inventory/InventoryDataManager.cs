using Fusion;
using System;
using Unity.VisualScripting;
using UnityEngine;

public delegate void OnItemGet();

public class InventoryDataManager : NetworkBehaviour
{
    [SerializeField] int maxStackCount;//아이템 최대 스택 개수
    GameObject uiCanvas;
    Canvas_Holder uiHolder;
    [Networked, Capacity(30)] public NetworkLinkedList<Item> itemList => default;
    public override void Spawned()
    {
        for (int i = 0; i < 29; i++)
        {
            itemList.Add(new Item
            {
                itemID = -1,
                count = 0,
                durability = 1
            });
        }
    }

    public Item_Panel[] inventorySlots;
    public GameObject inventoryItemPrefab;
    int selectedSlot = -1;
    int maxSlotCount = 9; // 총 슬롯 수
    public static InventoryDataManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
        uiCanvas = GameObject.Find("UI_Canvas");
        uiHolder = uiCanvas.GetComponent<Canvas_Holder>();
        int i = 0;
        while (i < 9)
        {
            inventorySlots[i] = uiHolder.hotBarUI.transform.GetChild(i).GetComponent<Item_Panel>();
            i++;
        }
        int index = 0;
        while (i < 29)
        {
            inventorySlots[i] = uiHolder.inventoryUI.transform.GetChild(index).GetComponent<Item_Panel>();
            i++;
            index++;
        }
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
        if (Input.inputString != null)
        {
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if (isNumber && number > 0 && number < 10)
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
        if (slot.item.isNull == false)
        {
            if (use)
            {
                slot.item.count--;
                if (slot.item.count <= 0)
                {
                    //null 설정
                    var item = itemList.Get(selectedSlot);
                    item.isNull = true;
                    item.count = 0;
                    itemList.Set(selectedSlot, item);
                    //itemList[selectedSlot] = null;
                }
                onInventoryUpdated?.Invoke();
            }
            return null;
        }
        return null;
    }

    // 아이템 획득
    public bool GetItem(int id, int amount)
    {
        //int id = scriptableData.itemID;
        // 이미 존재하는 아이템이면 개수만 증가
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].itemID != -1)
            {
                if (itemList[i].GetData().type == Item_Type.Equipment) maxStackCount = 1;
                else maxStackCount = 20;
                if (itemList[i].itemID == id && itemList[i].count < maxStackCount)
                {
                    var item = itemList.Get(i);
                    item.count += amount;
                    itemList.Set(i, item);
                    //itemList[i].count += amount;
                    onInventoryUpdated?.Invoke();
                    return true;
                }
            }

        }
        // 빈 슬롯 찾기
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].itemID == -1)
            {
                Item newItem = new Item { itemID = id, count = amount };
                itemList.Set(i, newItem);
                //itemList[i] = newItem;
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
            Debug.Log(itemList[index].GetData().name + " 버림!");
            var item = itemList.Get(index);
            item.count = 0;
            item.isNull = true;
            itemList.Set(index, item);
            //itemList[index] = null;
            onInventoryUpdated?.Invoke();
        }
    }

    // 아이템 소지 여부 확인
    public bool HaveItem(int id)
    {
        foreach (var item in itemList)
        {
            if (item.isNull == false && item.GetData().itemID == id)
                return true;
        }
        return false;
    }

    public NetworkLinkedList<Item> GetItemList()
    {
        return itemList;
    }

    public int GetSelectedIndex()
    {
        return selectedSlot;
    }
}