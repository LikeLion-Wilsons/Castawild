using UnityEngine;
using UnityEngine.EventSystems;

public class TrashArea : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        //if (!Canvas_Holder.instance.IsInventoryOpen()) return;
        var itemPanel = eventData.pointerDrag?.GetComponent<Item_Panel>();
        if (itemPanel == null) return;

        var inventory = itemPanel.inventory.GetComponent<UIInventory>();
        int index = inventory.GetComponent<UIInventory>().GetIndex(itemPanel);

        if (inventory.inventoryData.canvasHolder.IsInventoryOpen()) return;
        //클론 오브젝트일 경우(우클릭으로 나눠짐)
        if (itemPanel.draggedClone != null)
        {
            var clonePanel = itemPanel.draggedClone.GetComponent<Item_Panel>();
            if (clonePanel != null && clonePanel.isClone)
            {
                Destroy(clonePanel.gameObject);
                itemPanel.draggedClone = null; // 드래그 종료 처리
                                               //Destroy(itemPanel);
            }
        }
        
        else
        {
            inventory.inventoryData.ThrowItem(index);          // 아이템 제거

            var item = itemPanel.item;
            item.isNull = true;
            item.count = 0;

            itemPanel.SlotInit(item); // 슬롯 비우기
        }

        itemPanel.SetItemSlot();             // UI 갱신
    }
}
