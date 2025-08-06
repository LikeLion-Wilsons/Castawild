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

        if (Object.HasInputAuthority)
        {
            //NetworkObject playerObj = Runner.GetPlayerObject(Object.InputAuthority);
            //uiCanvas = playerObj.GetComponent<InventoryDataManager>().UICanvas;
            //uiInventory = uiCanvas.GetComponentInChildren<UIInventory>();
            //uiInventory.BindToInventoryData(this);
        }

        chest = GetComponent<Chest>();
        isSpawned = true;
        
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_SetItem(int index, Item item)
    {
        itemList.Set(index, item);
        Debug.Log(itemList[index]);
    }

    public void SwapItems(int indexA, int indexB)
    {
        if (indexA >= itemList.Count && indexB >= itemList.Count) return;
        Debug.Log("Swap_Chest");
        // 슬롯 수 부족할 경우 확장
        while (itemList.Count <= Mathf.Max(indexA, indexB))
        {
            var item = new Item { itemID = -1, count = 0 };
            itemList.Add(item);
        }

        var tempA = itemList[indexA];
        var tempB = itemList[indexB];

        itemList.Set(indexA, tempB);
        itemList.Set(indexB, tempA);

        if (Object.HasStateAuthority)
        {
            //RPC_UpdateInventoryUI();
        }
    }


}
