using UnityEngine;
using System.Collections.Generic;
using System;

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
                item.itemID = -1;
                item.count = 0;
            }
            //Debug.Log(i + " : " + items[i].itemID+" " + items[i].count);
            itemPanels[i].SlotInit(item);
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
            var item = new Item { itemID = -1, count = 0 };
            items.Add(item);

            inventoryData.RPC_SetItem(indexB, item);
            //items.Add(null);
        }

        var temp = items[indexA];
        items[indexA] = items[indexB];
        items[indexB] = temp;

        var tempA = items[indexA];
        var tempB = items[indexB];

        // 교환 후 Set 호출로 Fusion에 알려줌
        inventoryData.RPC_SetItem(indexA, tempA);
        inventoryData.RPC_SetItem(indexB, tempB);
        //items.Set(indexA, tempA);
        //items.Set(indexB, tempB);

        SetItemList();
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
}
