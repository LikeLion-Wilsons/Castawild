using Fusion;
using UnityEngine;

public class Campfire : InteractableObject
{
    [Networked] private bool CanOpen { get; set; } = true;
    public bool isFire { get; set; } = false;
    UI_Manager canvasHolder;
    [SerializeField] GameObject fireVFX;

    private void Awake()
    {
        interactableType = InteractableType.Box;
        isPlaceable = true;
    }

    private void Update()
    {
        fireVFX.SetActive(isFire);//불 On/Off

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
        canvasHolder.currentCampFire = gameObject;

        if (CanOpen)
        {
            canvasHolder.uiParts["Inventory"].Open();
            canvasHolder.uiParts["Campfire"].Open();
            CanOpen = false;
        }
    }

    public void SetFire(bool tof)
    {
        isFire = tof;
    }
}
