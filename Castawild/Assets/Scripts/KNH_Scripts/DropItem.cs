using Fusion;
using Test;
using UnityEngine.Splines;

public class DropItem : NetworkBehaviour, IInteractable
{
    public Item item;

    [Networked] private TickTimer reviveTimer { get; set; }

    public bool CanInteract()
    {
        return true;
    }

    public void Init(Item _item)
    {
        item = _item;
    }
    public void Interact(PlayerRef player)
    {
        RPC_Request(player);
    }

    //클라->서버
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_Request(PlayerRef player)
    {
        var playerObj = Runner.GetPlayerObject(player);
        var inven = playerObj.GetComponent<InventoryDataManager>();
        inven.GetItem(item.itemID, item.count);
        reviveTimer = TickTimer.CreateFromSeconds(Runner, 2f);

        Destroy(gameObject);
    }
}
