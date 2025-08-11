using Fusion;
using UnityEngine;

public class Campfire : InteractableObject
{
    [Networked] private bool CanOpen { get; set; } = true;
    public bool isFire { get; set; } = false;
    UI_Manager canvasHolder;
    [SerializeField] GameObject fireVFX;
    public Player player;
    InventoryDataManager inventoryData;

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
    public void FinishInteract() => CanOpen = true;

    public override void Interact(PlayerRef playerRef)
    {
        NetworkObject playerObj = Runner.GetPlayerObject(playerRef);

        player = playerObj.GetComponent<Player>();

        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        inventoryData = player.GetComponent<InventoryDataManager>();

        GetComponent<NetworkCampFire>().inventoryData = inventoryData;

        canvasHolder = inventoryData.canvasHolder;
        canvasHolder.currentCampFire = gameObject;

        if (CanOpen)
        {
            canvasHolder.uiParts["Inventory"].Open();
            canvasHolder.uiParts["Campfire"].Open();
            CanOpen = false;
        }
    }

    public void SetFireActive(bool tof)
    {
        isFire = tof;
        fireVFX.SetActive(tof);
    }
}
