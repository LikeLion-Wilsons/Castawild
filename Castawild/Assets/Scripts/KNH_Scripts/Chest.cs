using Fusion;
using System;
using UnityEngine;

public class Chest : InteractableObject
{
    [Networked] private bool CanOpen { get; set; } = true;
    Canvas_Holder canvasHolder;
    public Player player;
    ChestDataManager chestData;
    InventoryDataManager inventoryData;
    private void Awake()
    {
        interactableType = InteractableType.Box;
        isPlaceable = true;
    }

    public override bool CanInteract()
    {
        // Spawn되기 전에는 Networked 프로퍼티 접근 불가
        if (!GetComponent<ChestDataManager>().isSpawned)
            return false;

        return CanOpen;
    }
    public override void Interact(PlayerRef playerRef)
    {
        NetworkObject playerObj = Runner.GetPlayerObject(playerRef);

        player = playerObj.GetComponent<Player>();

        PlayerController playerController = playerObj.GetComponent<PlayerController>();

        canvasHolder = playerObj.GetComponent<InventoryDataManager>().canvasHolder;
        chestData = GetComponent<ChestDataManager>();
        inventoryData = player.GetComponent<InventoryDataManager>();
        //현재 열고 있는 상자 설정
        canvasHolder.SetOpenedChest(chestData);


        //chest -> inventory
        inventoryData.RPC_SetItemFromChest(chestData);
        //int index = 0;
        //for (int i = 29; i < 45; i++)
        //{
        //    inventoryData.RPC_SetItem(i, chestData.itemList[index]);
        //    index++;
        //}
        //inventoryData.RPC_UpdateInventoryUI();


        if (CanOpen)
        {
            canvasHolder.uiParts["Inventory"].Open(player.inputManager);
            canvasHolder.uiParts["Chest"].Open(player.inputManager);
            if (Object.HasStateAuthority)
                CanOpen = false;
        }
    }

    public void FinishInteract()
    {

        int index = 29;
        if (Object.HasStateAuthority)
        {
            CanOpen = true;
            //inventory -> chest

            for (int i = 0; i < 16; i++)
            {
                chestData.RPC_SetItem(i, inventoryData.itemList[index]);
                index++;
            }
            Debug.Log("inventory -> chest");
            inventoryData.RPC_UpdateInventoryUI();
        }
            
            //inventoryData.RPC_RequestStoreToChest(chestData);


        index = 0;
        //inventory 초기화
        Item item = new Item
        {
            itemID = -1,
            count = 0,
            durability = 1
        };
        for (int i = 29; i < 45; i++)
        {
            player.GetComponent<InventoryDataManager>().RPC_SetItem(i, item);
            index++;
        }
    }
    //// 클라이언트에서 호스트에 요청
    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    //public void RPC_RequestChestOpen()
    //{
    //    player.GetComponent<InventoryDataManager>().RPC_SetItemFromChest(chestData);
    //}

}
