using UnityEngine;
using System.Collections.Generic;

public class Inventory : UIPart
{
    public Item_Panel item_panel;
    public Transform content;

    List<Item_Panel> items = new List<Item_Panel>();
    Dictionary<int, Item> inventory_items = new Dictionary<int, Item>();

    int itemMaximumValue = 20;

    public GameObject itemClick;


    private void Start()
    {
        Init();
        InventoryManager.onItemGet += SetItemList;
        InventoryManager.onItemGet += SetInventory;
    }
    public void Init()
    {
        if (InventoryManager.item_List.Count >= itemMaximumValue)
            itemMaximumValue = InventoryManager.item_List.Count;

        for (int i = 0; i < itemMaximumValue; i++)
        {
            items.Add(content.GetChild(i).GetComponent<Item_Panel>());
        }

        int value = 0;

        foreach (var item in InventoryManager.item_List)
        {
            items[value].Init(item.Value, this);
            value++;
        }
        SetItemList();
        SetInventory();
    }

    public void SetItemList()
    {
        int value = 0;
        foreach (var item in InventoryManager.item_List)
        {
            if ((inventory_items.ContainsKey(item.Value.item_Data.itemID) == false)
                && items[value].parentPanel == null)
            {
                items[value].Init(item.Value, this);
                inventory_items.Add(item.Value.item_Data.itemID, item.Value);
            }
            value++;
        }
    }
    public void SetInventory()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetItem();//아이콘, 개수 설정
        }
    }

    public void SetItemClickAnimation(Item_Panel panel)
    {
        itemClick.gameObject.SetActive(true);
        itemClick.transform.SetParent(panel.transform);
        itemClick.transform.localPosition = Vector2.zero;
    }
}
