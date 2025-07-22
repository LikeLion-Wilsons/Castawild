using UnityEngine;
using System.Collections.Generic;

public class UIInventory : UIPart
{
    public Item_Panel item_panel;
    public Transform content;

    public List<Item_Panel> itemPanels = new List<Item_Panel>();

    public GameObject itemClick;
    private InventoryDataManager inventoryData;
    public void BindToInventoryData(InventoryDataManager data)
    {
        inventoryData = data;
        data.onInventoryUpdated += SetItemList;
        Init();
    }

    public void Init()
    {
        int itemMaximumValue = content.childCount;

        SetItemList();
    }

    //아이템을 얻을 때 실행
    public void SetItemList()
    {
        var items = inventoryData.itemList;

        for (int i = 0; i < itemPanels.Count; i++)
        {
            //Item item = (i < items.Count) ? items[i] : null;
            Item item = items[i];
            if (i < items.Count)
                item = items[i];
            else
            {
                item.isNull = true;
                item.count = 0;
            }

            itemPanels[i].SlotInit(item);
            itemPanels[i].SetItemSlot();
        }
    }
    public void RefreshUI()
    {
        for (int i = 0; i < itemPanels.Count; i++)
        {
            itemPanels[i].SetItemSlot();
        }
    }

    public void SwapItems(int indexA, int indexB)
    {
        var items = inventoryData.itemList;

        if (indexA >= items.Count && indexB >= items.Count) return;

        // 슬롯 수 부족할 경우 확장
        while (items.Count <= Mathf.Max(indexA, indexB))
        {
            var item = new Item { isNull = true, count = 0 };
            items.Add(item);
            //items.Add(null);
        }

        var temp = items[indexA];
        items[indexA] = items[indexB];
        items[indexB] = temp;

        SetItemList();
    }



    public void SetItemClickAnimation(Item_Panel panel)
    {
        if (inventoryData.canvasHolder.IsInventoryOpen()) return;
        itemClick.gameObject.SetActive(true);
        itemClick.transform.SetParent(panel.transform);
        itemClick.transform.localPosition = Vector2.zero;
    }

    public int GetIndex(Item_Panel slot)
    {
        for (int i = 0; i < itemPanels.Count; i++)
        {
            if (slot == itemPanels[i])
                return i;
        }
        return -1;
    }

    public Item_Panel GetItemByIndex(int index)
    {
        return itemPanels[index];
    }
}
