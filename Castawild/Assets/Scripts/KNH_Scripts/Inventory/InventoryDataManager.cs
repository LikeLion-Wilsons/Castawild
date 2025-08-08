using Fusion;
using System;
using UnityEngine;

public delegate void OnItemGet();

public class InventoryDataManager : NetworkBehaviour
{
    [SerializeField] int maxStackCount;//아이템 최대 스택 개수
    public UI_Manager canvasHolder;
    private UIInventory uiInventory;
    private UITable uiTable;
    [SerializeField] GameObject itemBox;
    private float nextScrollTime = 0f;
    public float scrollCooldown = 0.1f; // 100ms
    [SerializeField] GameObject playerUIPrefab; // 인스펙터에 연결
    public GameObject UICanvas;
   
    [Networked, Capacity(50)] public NetworkLinkedList<Item> itemList => default;

    private Player player;

    [Header("테스트용")]
    public GameObject chest;//나중에 삭제

    public override void Spawned()
    {
        ChangeSelectedSlot(0);

        if (Object.HasStateAuthority)
        {
            while (itemList.Count < 45)
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
            UICanvas = uiCanvas;
            uiInventory = uiCanvas.GetComponentInChildren<UIInventory>();
            uiInventory.BindToInventoryData(this);
            uiTable = uiCanvas.GetComponentInChildren<UITable>();
            uiTable.BindToInventoryData(this);

            canvasHolder = uiCanvas.GetComponent<UI_Manager>();
            player = GetComponent<Player>();
            canvasHolder.SetPlayer(player);

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
            index = 0;
            while (i < 45)
            {
                inventorySlots[i] = canvasHolder.chestUI.transform.GetChild(4).GetChild(index).GetComponent<Item_Panel>();
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
    int selectedSlot = 0;
    int maxSlotCount = 9; // 총 슬롯 수
    public static InventoryDataManager Instance { get; set; }
    public static event Action<int> onItemSelected;
    void ChangeSelectedSlot(int newValue)
    {
        if (Object.HasInputAuthority)
        {
            // 수정한 부분
            if (canvasHolder == null || canvasHolder.IsInventoryOpen())
                return;
            if (selectedSlot >= 0)
            {
                inventorySlots[selectedSlot].Deselect();
            }
            inventorySlots[newValue].Select();
            selectedSlot = newValue;
            onItemSelected?.Invoke(inventorySlots[newValue].item.itemID);

            // 수정한 부분
            if (inventorySlots[selectedSlot].IsEmpty())
                player.RemoveSelectedItem();
            else
                player.ApplySelectedItem(itemList[selectedSlot].itemID);
        }
    }


    public event Action onInventoryUpdated;//기존에 있던 아이템이 추가될 때


    private void Start()
    {
        // 수정한 부분
        // ChangeSelectedSlot(0);
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
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (Time.time >= nextScrollTime)
        {
            if (scroll > 0f)
            {
                if (player != null && player.toolStateManager.CurrentToolUseState == ToolAnimationState.Idle);
                int next = (selectedSlot - 1 + maxSlotCount) % maxSlotCount;
                ChangeSelectedSlot(next);
                nextScrollTime = Time.time + scrollCooldown;
            }
            else if (scroll < 0f)
            {
                if (player != null && player.toolStateManager.CurrentToolUseState == ToolAnimationState.Idle) ;
                int next = (selectedSlot + 1) % maxSlotCount;
                ChangeSelectedSlot(next);
                nextScrollTime = Time.time + scrollCooldown;
            }
        }

        //선택된 아이템 버리기
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RPC_ThrowItem(GetSelectedIndex());
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddItem(0, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddItem(1, 1);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RPC_UseSelectedItem(1);
        }

        //테스트용
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.B))
        {
            RPCRequestBuild();
        }
    }

    //테스트용
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPCRequestBuild()
    {
        PlayerRef playerRef = Runner.LocalPlayer;
        Runner.Spawn(chest, chest.transform.position, Quaternion.identity, null);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_UpdateInventoryUI()
    {
        uiInventory.SetItemList();
        uiTable.GetComponent<UITable>().SetTableUI();
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
    public void RPC_ThrowAllItem()
    {
        ThrowAllItem();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UseItem(int index, int count)
    {
        UseItem(index, count);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UseSelectedItem(int count)
    {
        UseItem(itemList[selectedSlot].itemID, count);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_GetItem(int itemId, int count)
    {
        AddItem(itemId, count);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetItem(int index, Item item)
    {
        itemList.Set(index, item);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetItemFromChest(ChestDataManager chestData)
    {
        int index = 0;
        for (int i = 29; i < 45; i++)
        {
            itemList.Set(i, chestData.itemList[index]);
            index++;
        }
        RPC_UpdateInventoryUI();
        Debug.Log("chest -> inventory");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestStoreToChest(ChestDataManager chestData)
    {
        int index = 29;
        for (int i = 0; i < 16; i++)
        {
            chestData.RPC_SetItem(i, itemList[index]);
            index++;
        }
        Debug.Log("RPC_RequestStoreToChest");
        RPC_UpdateInventoryUI();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetCanOpen(Chest chest ,bool tof)
    {
        chest.CanOpen = tof;
    }


    public Item_Scriptable GetSeletedItem(bool use)
    {
        Item_Panel slot = inventorySlots[selectedSlot];
        if (slot.item.itemID != -1)
        {
            if (use)
            {
                slot.item.count--;
                if (slot.item.count <= 0)
                {
                    //null 설정
                    var item = itemList.Get(selectedSlot);
                    item.itemID = -1;
                    item.count = 0;
                    itemList.Set(selectedSlot, item);
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
    public bool AddItem(int id, int amount)
    {
        if (HasInputAuthority & id == 201)
            player.RPC_NotifyHasArrow(true);

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

                if (Object.HasStateAuthority)
                {
                    RPC_UpdateInventoryUI();
                }
                return true;
            }
        }
        return false;

    }

    // 추가한 부분
    // 아이템 있는지 확인
    public bool HasItem(int id)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].itemID == id)
            {
                return true;
            }
        }

        return false;
    }

    public void SwapItems(int indexA, int indexB)
    {
        if (indexA >= itemList.Count || indexB >= itemList.Count) return;

        //Debug.Log("Swap " + indexA + " " + indexB);

        // 슬롯 수 부족할 경우 확장
        while (itemList.Count <= Mathf.Max(indexA, indexB))
        {
            var item = new Item { itemID = -1, count = 0 };
            itemList.Add(item);
        }

        //교환
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

            var playerObj = Runner.GetPlayerObject(Object.InputAuthority);
            var box = Runner.Spawn(itemBox, playerObj.transform.position + new Vector3(1, 0.5f, 1), Quaternion.identity, null, (runner, o) =>
            {
                o.GetComponent<DropItem>().Init(item);
            });

            item.itemID = -1;
            item.count = 0;
            itemList.Set(index, item);
            if (Object.HasStateAuthority)
            {
                RPC_UpdateInventoryUI();
            }
        }
    }

    //들고 있는 모든 아이템 버리기
    public void ThrowAllItem()
    {
        for(int i = 0; i< 29; i++)
        {
            if (itemList[i].itemID != -1)
            {
                RPC_ThrowItem(i);
            }
        }
    }

    public void UseItem(int id, int count)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].itemID == id)
            {
                var item = itemList.Get(i);
                if (item.count - count >= 0)
                {
                    item.count -= count;
                    itemList.Set(i, item);
                    break;
                }
                else
                {
                    count -= item.count;
                    item.count = 0;
                    itemList.Set(i, item);
                }

            }
        }
        if (Object.HasStateAuthority)
        {
            RPC_UpdateInventoryUI();
        }
    }



    // 아이템 소지 수량 확인
    public int GetItemCount(int id)
    {
        int count = 0;
        foreach (var item in itemList)
        {
            if (item.itemID != -1 && item.GetData().itemID == id)
                count += item.count;
        }
        return count;
    }

    public NetworkLinkedList<Item> GetItemList()
    {
        return itemList;
    }

    public int GetSelectedIndex()
    {
        return selectedSlot;
    }

    public int GetSelectedItemID()
    {
        return itemList[selectedSlot].itemID;
    }
}