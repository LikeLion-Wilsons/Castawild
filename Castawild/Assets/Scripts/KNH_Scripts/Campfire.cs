using Fusion;
using UnityEngine;

public class Campfire : InteractableObject
{
    [Networked] private bool CanOpen { get; set; } = true;
    UI_Manager canvasHolder;

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

        Player player = playerObj.GetComponent<Player>();

        PlayerController playerController = playerObj.GetComponent<PlayerController>();

        canvasHolder = playerObj.GetComponent<InventoryDataManager>().canvasHolder;

        if (CanOpen)
        {
            canvasHolder.uiParts["Inventory"].Open(player.inputManager);
            canvasHolder.uiParts["Campfire"].Open(player.inputManager);
            CanOpen = false;
        }
    }
}
