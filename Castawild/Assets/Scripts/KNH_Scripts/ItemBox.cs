using Fusion;
using UnityEngine;

public class ItemBox : InteractableObject
{
    [Networked] private bool canInteract { get; set; } = true;
    public Item item;

    private void Awake()
    {
        interactableType = InteractableType.Item;
        isPlaceable = true;
    }

    public void Init(Item _item)
    {
        item = _item;
    }

    public override bool CanInteract() => canInteract;

    public override void Interact(PlayerRef playerRef)
    {
        text = item.GetData().name;//아이템 이름 설정

        NetworkObject playerObj = Runner.GetPlayerObject(playerRef);

        Player player = playerObj.GetComponent<Player>();
        InventoryDataManager inventoryData =  player.GetComponent<InventoryDataManager>();
        inventoryData.AddItem(item.itemID, item.count);//아이템 획득
        Destroy(gameObject);
    }
}
