using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditorInternal.Profiling.Memory.Experimental;

public class Inventory : UIPart
{
    public Item_Panel item_panel;
    public Transform content;

    List<Item_Panel> items = new List<Item_Panel>();

    int itemMaximumValue = 24;

    private void OnEnable()
    {
        Init();
        SetInventory();
    }
    public void Init()
    {
        if (InventoryManager.item_List.Count >= itemMaximumValue)
            itemMaximumValue = InventoryManager.item_List.Count;

        for (int i = 0; i < itemMaximumValue; i++)
        {
            var go = Instantiate(item_panel, content);
            go.gameObject.SetActive(true);
            items.Add(go);
        }

        int value = 0;

        foreach (var item in InventoryManager.item_List)
        {
            items[value].Init(item.Value);
            value++;
        }

        SetInventory();
    }
    public void SetInventory()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetItem();//아이콘, 개수 설정
        }
    }
}
