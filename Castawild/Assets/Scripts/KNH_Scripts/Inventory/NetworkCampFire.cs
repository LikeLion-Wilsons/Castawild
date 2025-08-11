using Fusion;

public class NetworkCampFire : NetworkBehaviour
{
    [Networked] public Item cookPotItem { get; set; }
    [Networked] public Item resultItem { get; set; }
    Player player;
    Campfire campfire;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Item item = new Item
            {
                itemID = -1,
                count = 0,
                durability = 1
            };
            cookPotItem = item;
            resultItem = item;
        }
        player = GetComponent<Campfire>().player;

        campfire = GetComponent<Campfire>();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetCookPotItem(Item item)
    {
        cookPotItem = item;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetResultItem(Item item)
    {
        resultItem = item;
    }
}