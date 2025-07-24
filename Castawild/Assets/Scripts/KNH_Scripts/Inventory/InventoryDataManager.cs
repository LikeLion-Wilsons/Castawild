using Fusion;
using System;
using UnityEngine;

public delegate void OnItemGet();

public class InventoryDataManager : NetworkBehaviour
{
    [SerializeField] int maxStackCount;//아이템 최대 스택 개수
    public Canvas_Holder canvasHolder;
    private UIInventory uiInventory;
    [SerializeField] GameObject playerUIPrefab; // 인스펙터에 연결
    [Networked, Capacity(30)] public NetworkLinkedList<Item> itemList => default;
    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            while (itemList.Count < 29)
            {
                itemList.Add(new Item
                {
                    itemID = -1,
                    count = 0,
                    durability = 1
                });
            }

        }

        if (Object.HasInputAuthority)
        {
            // 본인의 UI만 생성
            GameObject uiCanvas = Instantiate(playerUIPrefab);
            uiCanvas.transform.SetParent(null); // 루트로 이동
            uiInventory = uiCanvas.GetComponentInChildren<UIInventory>();
            uiInventory.BindToInventoryData(this);

            canvasHolder = uiCanvas.GetComponent<Canvas_Holder>();

            int i = 0;
            while (i < 9)
            {
                inventorySlots[i] = canvasHolder.hotBarUI.transform.GetChild(i).GetComponent<Item_Panel>();
                i++;
            }
            int index = 0;
            while (i < 29)
            {
                inventorySlots[i] = canvasHolder.inventoryUI.transform.GetChild(index).GetComponent<Item_Panel>();
                i++;
                index++;
            }

            for (int k = 0; k < inventorySlots.Length; k++)
            {
                inventorySlots[k].GetComponent<Item_Panel>().BindToInventoryData(this);
            }

        }
    }


    public Item_Panel[] inventorySlots;
    public GameObject inventoryItemPrefab;
    int selectedSlot = -1;
    int maxSlotCount = 9; // 총 슬롯 수
    public static InventoryDataManager Instance { get; set; }

    void ChangeSelectedSlot(int newValue)
    {
        if (Object.HasInputAuthority)
        {
            if (canvasHolder.IsInventoryOpen()) return;
            if (selectedSlot >= 0)
            {
                inventorySlots[selectedSlot].Deselect();
            }
            inventorySlots[newValue].Select();
            selectedSlot = newValue;
        }
    }


    public event Action onInventoryUpdated;//기존에 있던 아이템이 추가될 때



    private void Start()
    {
        //ChangeSelectedSlot(0);
    }

    public override void FixedUpdateNetwork()
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
        //선택된 아이템 버리기
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RPC_ThrowItem(GetSelectedIndex());
        }

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    void RPC_UpdateInventoryUI()
    {
        uiInventory.SetItemList();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SwapItems(int indexA, int indexB)
    {
        SwapItems(indexA, indexB);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ThrowItem(int index)
    {
        ThrowItem(index);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetItem(int index, Item item)
    {
        itemList.Set(index, item);
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
                if (Object.HasStateAuthority)
                {
                    RPC_UpdateInventoryUI();
                }

            }
            return null;
        }
        return null;
    }

    // 아이템 획득
    public bool GetItem(int id, int amount)
    {
        //Debug.Log("GetItem");

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

                    onInventoryUpdated?.Invoke();
                    if (Object.HasStateAuthority)
                    {
                        RPC_UpdateInventoryUI();
                    }

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
                onInventoryUpdated?.Invoke();

                if (Object.HasStateAuthority)
                {
                    RPC_UpdateInventoryUI();
                }
                return true;
            }
        }
        return false;

    }

    public void SwapItems(int indexA, int indexB)
    {
        if (indexA >= itemList.Count && indexB >= itemList.Count) return;
        Debug.Log("Swap");
        // 슬롯 수 부족할 경우 확장
        while (itemList.Count <= Mathf.Max(indexA, indexB))
        {
            var item = new Item { itemID = -1, count = 0 };
            itemList.Add(item);
        }

        var tempA = itemList[indexA];
        var tempB = itemList[indexB];

        itemList.Set(indexA, tempB);
        itemList.Set(indexB, tempA);

        if (Object.HasStateAuthority)
        {
            RPC_UpdateInventoryUI();
        }
    }

    //아이템 버리기
    public void ThrowItem(int index)
    {
        if (index >= 0 && index < itemList.Count)
        {
            if (itemList[index].itemID == -1) return;
            Debug.Log(itemList[index].GetData().name + " 버림!");
            var item = itemList.Get(index);
            item.itemID = -1;
            item.count = 0;
            itemList.Set(index, item);
            if (Object.HasStateAuthority)
            {
                RPC_UpdateInventoryUI();
            }
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