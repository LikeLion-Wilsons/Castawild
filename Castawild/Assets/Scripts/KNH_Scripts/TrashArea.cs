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

        InventoryDataManager.Instance.ThrowItem(index);          // 아이템 제거
        itemPanel.SlotInit(null); // 슬롯 비우기
        itemPanel.SetItemSlot();             // UI 갱신
    }
}
