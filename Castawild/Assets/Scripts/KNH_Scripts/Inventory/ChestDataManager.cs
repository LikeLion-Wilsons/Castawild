using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class ChestDataManager : NetworkBehaviour
{
    [Networked, Capacity(20)] public NetworkLinkedList<Item> itemList => default;
    GameObject uiCanvas;
    UIInventory uiInventory;
    [SerializeField]Player player;
    Chest chest;

    public bool isSpawned = false;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        { 
            while (itemList.Count < 16)
            {
                itemList.Add(new Item
                {
                    itemID = -1,
                    count = 0,
                    durability = 1
                });
            }
            Debug.Log("Spawned");
        }
        player = GetComponent<Chest>().player;

        chest = GetComponent<Chest>();
        isSpawned = true;
        
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetItem(int index, Item item)
    {
        itemList.Set(index, item);
    }
}
