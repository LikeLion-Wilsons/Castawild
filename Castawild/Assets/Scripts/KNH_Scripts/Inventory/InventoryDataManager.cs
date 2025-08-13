using Fusion;
using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public delegate void OnItemGet();

public class InventoryDataManager : NetworkBehaviour
{
    [SerializeField] int maxStackCount;//아이템 최대 스택 개수
    public UI_Manager canvasHolder;
    public UIInventory uiInventory;
    private UITable uiTable;
    [SerializeField] GameObject itemBox;
    private float nextScrollTime = 0f;
    public float scrollCooldown = 0.1f; // 100ms
    [SerializeField] GameObject playerUIPrefab; // 인스펙터에 연결
    public GameObject UICanvas;

    [Networked, Capacity(50)] public NetworkLinkedList<Item> itemList => default;

    private Player player;
    public Item_Panel[] inventorySlots;

    public override void Spawned()
    {
        inventorySlots = new Item_Panel[47];
        ChangeSelectedSlot(0);

        if (Object.HasStateAuthority)
        {
            while (itemList.Count < 47)
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

            #region item slot Init
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


            inventorySlots[45] = canvasHolder.campfireUI.GetComponent<UICampfire>().cookPot;
            inventorySlots[46] = canvasHolder.campfireUI.GetComponent<UICampfire>().result;


            for (int k = 0; k < inventorySlots.Length; k++)
            {
                inventorySlots[k].GetComponent<Item_Panel>().BindToInventoryData(this);
            }

            #endregion
        }
    }


    public GameObject inventoryItemPrefab;
    [Networked] public int selectedSlot { get; set; } = 0;
    int maxSlotCount = 9; // 총 슬롯 수
    public static InventoryDataManager Instance { get; set; }
    public static event Action<int> onItemSelected;
    public static event Action onInventoryUpdated;

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
            RPC_SetSelectedSlot(newValue);

            if (inventorySlots[selectedSlot].IsEmpty())
                player.Client_RemoveSelectedItem();
            player.Client_ApplySelectedItem(itemList[selectedSlot].itemID);
            Debug.Log("ChangeSelectedSlot " + selectedSlot);
            onItemSelected?.Invoke(inventorySlots[selectedSlot].item.itemID);
        }
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
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (Time.time >= nextScrollTime)
        {
            if (scroll > 0f)
            {
                //if (player != null && player.toolStateManager.CurrentToolState == ToolState.Idle) ;
                if (!player.flagManager.IsUsingTool)
                {
                    int next = (selectedSlot - 1 + maxSlotCount) % maxSlotCount;
                    ChangeSelectedSlot(next);
                    nextScrollTime = Time.time + scrollCooldown;
                }
            }
            else if (scroll < 0f)
            {
                //if (player != null && player.toolStateManager.CurrentToolState == ToolState.Idle) ;
                if (!player.flagManager.IsUsingTool)
                {
                    int next = (selectedSlot + 1) % maxSlotCount;
                    ChangeSelectedSlot(next);
                    nextScrollTime = Time.time + scrollCooldown;
                }
            }
        }

        //선택된 아이템 버리기
        if (Input.GetKeyDown(KeyCode.Q) && HasInputAuthority)
            RPC_ThrowItem(GetSelectedIndex());

        #region cheat
        //아이템 획득 치트
        if (Input.GetKeyDown(KeyCode.Alpha1))
            AddItem(0, 5);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            AddItem(1, 5);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            AddItem(2, 5);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            AddItem(3, 5);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            AddItem(4, 5);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            AddItem(5, 5);
        if (Input.GetKeyDown(KeyCode.Alpha7))
            AddItem(6, 5);
        #endregion
    }


    #region RPC
    //인벤토리 UI 업데이트
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_UpdateInventoryUI()
    {
        uiInventory.SetItemList();
        uiTable.GetComponent<UITable>().SetTableUI();
    }

    //아이템 위치 교환
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SwapItems(int indexA, int indexB)
    {
        SwapItems(indexA, indexB);
    }
    //index 슬롯의 아이템 버리기
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ThrowItem(int index)
    {
        ThrowItem(index);
    }

    //갖고 있는 모든 아이템 버리기
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ThrowAllItem()
    {
        ThrowAllItem();
    }

    //ID를 기반으로 아이템 사용하기
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UseItem(int id, int count)
    {
        UseItem(id, count);
    }
    //선택된 슬롯에 있는 아이템 사용하기
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UseSelectedItem(int count)
    {
        UseItem(itemList[selectedSlot].itemID, count);
    }
    //선택된 슬롯에 있는 아이템 내구도 설정
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SubtractDurability(float amount)
    {
        Debug.Log("RPC_SubtractDurability" + selectedSlot);
        var item = itemList.Get(selectedSlot);
        if (item.itemID == -1) return;
        //내구도 감소
        item.durability -= amount;
        if (item.durability <= 0)
        {
            item.count = 0;
            item.itemID = -1;
        }
        itemList.Set(selectedSlot, item);

        RPC_UpdateInventoryUI();

    }

    //아이템 얻기
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetSelectedSlot(int index)
    {
        selectedSlot = index;
    }

    //아이템 얻기
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_GetItem(int itemId, int count)
    {
        AddItem(itemId, count);
    }
    //아이템 데이터 설정
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetItem(int index, Item item)
    {
        itemList.Set(index, item);
    }
    //상자로부터 아이템 데이터 받아오기
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
    //인벤토리에서 상자로 데이터 보내기
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
    //상자가 열 수 있는 상태인지 설정
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetCanOpen(Chest chest, bool tof)
    {
        chest.CanOpen = tof;
    }

    //모닥불이 열 수 있는 상태인지 설정
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetCanOpen(Campfire campfire, bool tof)
    {
        campfire.CanOpen = tof;
    }


    //모닥불로부터 아이템 데이터 받아오기
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetItemFromCampfire(NetworkCampFire campfire)
    {
        itemList.Set(45, campfire.cookPotItem);
        itemList.Set(45, campfire.resultItem);
        RPC_UpdateInventoryUI();
        Debug.Log("campfire -> inventory");
    }
    //모닥불에서 상자로 데이터 보내기
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestStoreToCampfire(NetworkCampFire campfire)
    {
        campfire.RPC_SetCookPotItem(itemList[45]);
        campfire.RPC_SetResultItem(itemList[46]);
        Debug.Log("inventory->campfire");
        RPC_UpdateInventoryUI();
    }

    #endregion
    //아이템 비우기
    public void ClearItem(Item item)
    {
        item.itemID = -1;
        item.count = 0;
        if (inventorySlots[selectedSlot].IsEmpty())
            player.Client_RemoveSelectedItem();
        RPC_UpdateInventoryUI();
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
    public bool AddItem(int id, int amount, float dur = 1f)
    {
        if (HasStateAuthority)
        {
            if (player == null)
                player = GetComponent<Player>();

            if (id == 201)
                player.Host_SetHasArrow(true);
            else if (id == 202)
                player.Host_SetHasPebble(true);
        }
        else if (HasInputAuthority)
        {
            if (id == 201)
                player.RPC_RequestSetHasArrow(true);
            else if (id == 202)
                player.RPC_RequestSetHasPebble(true);
        }

        // 이미 존재하는 아이템이면 개수만 증가
        for (int i = 0; i < 29; i++)
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
        for (int i = 0; i < 29; i++)
        {
            if (itemList[i].itemID == -1)
            {
                Debug.Log(dur);
                Item newItem = new Item { itemID = id, count = amount, durability = dur };
                itemList.Set(i, newItem);

                if (Object.HasStateAuthority)
                {
                    RPC_UpdateInventoryUI();
                }

                // 수정한 부분
                if (Object.HasInputAuthority)
                {
                    if (inventorySlots[selectedSlot].IsEmpty())
                        player.Client_RemoveSelectedItem();
                    player.Client_ApplySelectedItem(itemList[selectedSlot].itemID);
                    onItemSelected?.Invoke(inventorySlots[selectedSlot].item.itemID);
                }
                return true;
            }
        }
        Debug.Log("인벤토리 가득 참");
        return false;
    }

    public void SwapItems(int indexA, int indexB)
    {
        //indexA : 이동 전 슬롯
        //indexB : 이동 후 슬롯
        if (indexA >= itemList.Count || indexB >= itemList.Count) return;

        if (indexA > 44 && itemList[indexB].itemID != 6) return;//모닥불에는 생고기만 이동 가능 
        if (indexA == 46) return;//result슬롯으로는 이동 불가능

        //Debug.Log("Swap " + indexA + " " + indexB);

        // 슬롯 수 부족할 경우 확장
        while (itemList.Count <= Mathf.Max(indexA, indexB))
        {
            var item = new Item { itemID = -1, count = 0, durability = 1 };
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
            item.durability = 0;
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
        for (int i = 0; i < 29; i++)
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
                if (item.count - count > 0)
                {
                    item.count -= count;
                    itemList.Set(i, item);
                    break;
                }
                else
                {
                    item.itemID = -1;
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