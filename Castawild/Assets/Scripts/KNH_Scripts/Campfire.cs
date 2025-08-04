using Fusion;
using UnityEngine;

public class Campfire : InteractableObject
{
    [Networked] private bool CanOpen { get; set; } = true;
    Canvas_Holder canvasHolder;

    private void Awake()
    {
        interactableType = InteractableType.Box;
        isPlaceable = true;
    }

    private void Update()
    {
        if (canvasHolder == null) return;
        if (canvasHolder.uiParts["Inventory"].IsOpen())
            CanOpen = false;
        else
            CanOpen = true;

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
            canvasHolder.uiParts["Inventory"].Toggle();
            canvasHolder.uiParts["Campfire"].Toggle();
            CanOpen = false;
        }
    }
}
