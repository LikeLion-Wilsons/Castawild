using Fusion;
using System;
using UnityEngine;

public class Chest : InteractableObject
{
    [Networked] private bool CanOpen { get; set; } = true;
    Canvas_Holder canvasHolder;
    public Player player;
    ChestDataManager chestData;
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
    public void FinishInteract() => CanOpen = true;

    public override void Interact(PlayerRef playerRef)
    {
        NetworkObject playerObj = Runner.GetPlayerObject(playerRef);

        player = playerObj.GetComponent<Player>();

        PlayerController playerController = playerObj.GetComponent<PlayerController>();

        canvasHolder = playerObj.GetComponent<InventoryDataManager>().canvasHolder;
        chestData = GetComponent<ChestDataManager>();
        //현재 열고 있는 상자 설정
        canvasHolder.SetOpenedChest(chestData);

        int index = 0;
        for (int i = 29; i < 45; i++)
        {
            player.GetComponent<InventoryDataManager>().RPC_SetItem(i, chestData.itemList[index]);
            index++;
        }


        if (CanOpen)
        {
            canvasHolder.uiParts["Inventory"].Open(player.inputManager);
            canvasHolder.uiParts["Chest"].Open(player.inputManager);
            CanOpen = false;
        }
    }
}
