using Fusion;
using NUnit.Framework.Interfaces;
using UnityEngine;

public class Campfire : InteractableObject
{
    [Networked] public bool CanOpen { get; set; } = true;
    public bool isFire { get; set; } = false;
    UI_Manager canvasHolder;
    [SerializeField] GameObject fireVFX;
    public Player player;
    InventoryDataManager inventoryData;
    NetworkCampFire networkCampfire;

    private void Awake()
    {
        interactableType = InteractableType.Box;
        isPlaceable = true;
    }

    private void Update()
    {
        
        if (canvasHolder == null) return;
        bool isInventoryOpen = canvasHolder.uiParts["Inventory"].IsOpen();
        CanOpen = !isInventoryOpen;
    }

    public override bool CanInteract() => CanOpen;
    public void FinishInteract()
    {
        int index = 29;

        if (Object.HasStateAuthority)        //호스트에서
        {
            CanOpen = true;

            Debug.Log("호스트 inventory -> campfire");

            networkCampfire.cookPotItem = inventoryData.itemList[45];
            networkCampfire.resultItem = inventoryData.itemList[46];
            inventoryData.RPC_UpdateInventoryUI();
        }
        else if (player.HasInputAuthority) //클라이언트에서
        {
            inventoryData.RPC_SetCanOpen(this, true);
            Debug.Log("클라이언트 inventory -> campfire");
            inventoryData.RPC_RequestStoreToCampfire(networkCampfire);
        }

        index = 0;
        //inventory 초기화
        Item item = new Item
        {
            itemID = -1,
            count = 0,
            durability = 1
        };
        for (int i = 45; i < 47; i++)
        {
            player.GetComponent<InventoryDataManager>().RPC_SetItem(i, item);
            index++;
        }
    }

    public override void Interact(PlayerRef playerRef)
    {
        NetworkObject playerObj = Runner.GetPlayerObject(playerRef);

        player = playerObj.GetComponent<Player>();

        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        inventoryData = player.GetComponent<InventoryDataManager>();
        networkCampfire = GetComponent<NetworkCampFire>();
        networkCampfire.inventoryData = inventoryData;

        canvasHolder = inventoryData.canvasHolder;
        canvasHolder.currentCampFire = gameObject;

        if (CanOpen)
        {
            canvasHolder.uiParts["Inventory"].Open();
            canvasHolder.uiParts["Campfire"].Open();
            if (Object.HasStateAuthority)
                CanOpen = false;
            else if (player.HasInputAuthority)
                inventoryData.RPC_SetCanOpen(this, false);
        }
    }

    public void SetFireActive(bool tof)
    {
        isFire = tof;
        fireVFX.SetActive(tof);
    }
}
