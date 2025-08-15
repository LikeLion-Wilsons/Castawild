using Fusion;


public class DropItem : InteractableObject
{
    [Networked] private bool canInteract { get; set; } = true;
    public Item item;

    public override void Spawned()
    {

    }

    private void Awake()
    {
        interactableType = InteractableType.Item;
        isPlaceable = true;
    }

    public void Init(Item _item)
    {
        item = _item;
        text = item.GetData().itemName;//아이템 이름 설정
    }

    public override bool CanInteract() => canInteract;

    public override void Interact(PlayerRef playerRef)
    {
        NetworkObject playerObj = Runner.GetPlayerObject(playerRef);

        Player player = playerObj.GetComponent<Player>();
        InventoryDataManager inventoryData = player.GetComponent<InventoryDataManager>();
        inventoryData.AddItem(item.itemID, item.count,item.durability);//아이템 획득
        Runner.Despawn(GetComponent<NetworkObject>());
    }
}
