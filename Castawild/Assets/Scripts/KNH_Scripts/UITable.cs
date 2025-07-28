using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITable : UIPart
{
    [SerializeField] GameObject button;
    [SerializeField] Transform parent;
    public Item_Scriptable selectedItem;
    public GameObject descPanel;
    public GameObject craftButton;
    public InventoryDataManager inventoryData;
    public bool canCreate = true;

    public void BindToInventoryData(InventoryDataManager data)
    {
        inventoryData = data;
    }

    void Start()
    {
        List<Item_Scriptable> itemDataList = ItemDataBase.Instance.items;
        //제작 아이템 목록
        for (int i = 0; i<itemDataList.Count; i++)
        {
            if (itemDataList[i].itemID >= 300)
            {
                GameObject go = Instantiate(button);
                go.transform.SetParent(parent);
                go.GetComponent<CraftableItemUI>().Init(descPanel, ItemDataBase.Instance.GetItemByID(itemDataList[i].itemID));
            }
        }
    }

    public void Craft()
    {
        if (canCreate)
        {
            craftButton.GetComponent<Image>().color = Color.green;
            inventoryData.GetItem(selectedItem.itemID, 1);
            for(int i=0;i< selectedItem.ingredient.Count; i++)
            {
                inventoryData.UseItem(selectedItem.ingredient[i].itemID, selectedItem.ingredientCount[i]);
            }

        }
        else
        {
            craftButton.GetComponent<Image>().color = Color.red;
        }
    }
}
