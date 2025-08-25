using UnityEngine;
using System.Collections.Generic;
using System;
using Test;

public class UIInventory : UIPart
{
    public Item_Panel item_panel;
    public Transform content;

    public List<Item_Panel> itemPanels = new List<Item_Panel>();

    public GameObject itemClick;
    public InventoryDataManager inventoryData;

    public void BindToInventoryData(InventoryDataManager data)
    {
        inventoryData = data;
        InventoryDataManager.onInventoryUpdated += SetItemList;
        Init();
    }

    public void Init()
    {
        int itemMaximumValue = content.childCount;

        SetItemList();
    }

    public void SetitemSlot(int index, int count, Item item)
    {
        var items = inventoryData.itemList;
        item.count = count;
        itemPanels[index].SlotInit(item);
        itemPanels[index].SetItemSlot();
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
                item.itemID = -1;
                item.count = 0;
                item.durability = 1;
            }
            //Debug.Log(i + " : " + items[i].itemID+" " + items[i].count);
            itemPanels[i].SlotInit(item);
            itemPanels[i].SetItemSlot();
        }
    }

    public void SetItemClickAnimation(Item_Panel panel)
    {
        if (!inventoryData.canvasHolder.IsInventoryOpen()) return;
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


    //버튼에서 호출하는 메서드
    public void SortButton()
    {
        inventoryData.RPC_RequestInventorySort();
    }

}
