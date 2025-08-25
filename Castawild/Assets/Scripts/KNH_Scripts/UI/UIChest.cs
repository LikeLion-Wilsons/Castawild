using UnityEngine;

public class UIChest : UIPart
{
    [SerializeField]
    private UIInventory inventoryUI;
    InventoryDataManager inventoryData;
    public void ChestSortButton()
    {
        inventoryData = inventoryUI.inventoryData;
        inventoryData.RPC_RequestChestSort();
    }
}
