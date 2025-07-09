using UnityEngine;
using System.Collections.Generic;

public class UIInventory : UIPart
{
    public Item_Panel item_panel;
    public Transform content;

    public List<Item_Panel> itemPanels = new List<Item_Panel>();

    public GameObject itemClick;

    private void Start()
    {
        InventoryDataManager.onInventoryUpdated += SetItemList;
        Init();
    }
    public void Init()
    {
        //items.Clear();
        itemPanels.Clear();

        int itemMaximumValue = content.childCount;

        //아이템 슬롯 할당
        for (int i = 0; i < itemMaximumValue; i++)
        {
            itemPanels.Add(content.GetChild(i).GetComponent<Item_Panel>());
        }
        SetItemList();
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
        var items = InventoryDataManager.Instance.itemList;

        if (indexA >= items.Count && indexB >= items.Count) return;

        // 슬롯 수 부족할 경우 확장
        while (items.Count <= Mathf.Max(indexA, indexB))
        {
            items.Add(null);
        }

        var temp = items[indexA];
        items[indexA] = items[indexB];
        items[indexB] = temp;

        SetItemList();
    }

    //아이템을 얻을 때 실행
    public void SetItemList()
    {
        var items = InventoryDataManager.Instance.itemList;

        for (int i = 0; i < itemPanels.Count; i++)
        {
            Item item = (i < items.Count) ? items[i] : null;
            itemPanels[i].SlotInit(item);
            itemPanels[i].SetItemSlot();
        }
    }

    public void SetItemClickAnimation(Item_Panel panel)
    {
        itemClick.gameObject.SetActive(true);
        itemClick.transform.SetParent(panel.transform);
        itemClick.transform.localPosition = Vector2.zero;
    }
}
