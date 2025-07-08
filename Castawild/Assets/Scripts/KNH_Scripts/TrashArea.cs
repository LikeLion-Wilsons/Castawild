using UnityEngine;
using UnityEngine.EventSystems;

public class TrashArea : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var itemPanel = eventData.pointerDrag?.GetComponent<Item_Panel>();
        if (itemPanel == null) return;

        int index = itemPanel.transform.GetSiblingIndex();
        var inventory = itemPanel.inventory.GetComponent<UIInventory>();

        InventoryDataManager.Instance.ThrowItem(inventory.items[index].item_Data.itemID);          // 아이템 제거
        inventory.items[index] = null;  
        itemPanel.SlotInit(null, inventory); // 슬롯 비우기
        itemPanel.SetItemSlot();             // UI 갱신
    }
}
