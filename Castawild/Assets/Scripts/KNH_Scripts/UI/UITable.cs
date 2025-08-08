using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITable : UIPart
{
    [SerializeField] GameObject button;
    [SerializeField] Transform parent;
    [SerializeField] GameObject ingredientPanel;
    [SerializeField] GameObject itemName;
    [SerializeField] GameObject itemDesc;

    public Item_Scriptable selectedItem;
    public GameObject descPanel;
    public GameObject craftButton;
    public InventoryDataManager inventoryData;
    public bool canCreate = true;

    public void BindToInventoryData(InventoryDataManager data)
    {
        inventoryData = data;
        //data.onInventoryUpdated += craftButton.GetComponent<CraftableItemUI>().SetTableUI;
    }

    void Start()
    {
        List<Item_Scriptable> itemDataList = ItemDataBase.Instance.items;
        //제작 아이템 목록
        for (int i = 0; i<itemDataList.Count; i++)
        {
            if (itemDataList[i].itemID >= 200)
            {
                GameObject go = Instantiate(button);
                go.transform.SetParent(parent);
                go.GetComponent<CraftableItemUI>().Init(descPanel, ItemDataBase.Instance.GetItemByID(itemDataList[i].itemID));
            }
        }
    }

    private void Update()
    {
        if(canCreate) craftButton.GetComponent<Image>().color = Color.green;
        else craftButton.GetComponent<Image>().color = Color.red;
    }

    public void Craft()
    {
        if (inventoryData.canvasHolder.isDragging == true) return;//드래그 중에 제작 불가

        if (canCreate)
        {
            inventoryData.RPC_GetItem(selectedItem.itemID, 1);
            for(int i=0;i< selectedItem.ingredient.Count; i++)
            {
                inventoryData.RPC_UseItem(selectedItem.ingredient[i].itemID, selectedItem.ingredientCount[i]);
            }

        }
    }

    public void SetTableUI()
    {
        if (inventoryData.canvasHolder.isDragging == true) return;

        if (selectedItem == null) return;
        //텍스트 설정
        itemName.GetComponent<TextMeshProUGUI>().text = selectedItem.itemName;
        itemDesc.GetComponent<TextMeshProUGUI>().text = selectedItem.description;

        Transform content = descPanel.transform.GetChild(0);

        // 기존 재료 아이콘 삭제
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        if (selectedItem.ingredient == null) return;//재료 없으면 무시

        canCreate = true;
        for (int i = 0; i < selectedItem.ingredient.Count; i++)
        {
            //재료 아이콘 추가
            GameObject go = Instantiate(ingredientPanel);
            go.transform.SetParent(content);
            go.GetComponent<Image>().sprite = selectedItem.ingredient[i].image;

            //개수 반영
            TextMeshProUGUI countText = go.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            int currentCount = inventoryData.GetItemCount(selectedItem.ingredient[i].itemID);
            int needCount = selectedItem.ingredientCount[i];
            countText.text = currentCount + "/" + needCount.ToString();
            //텍스트 색상 설정
            if (currentCount >= needCount) countText.color = Color.white;
            else
            {
                countText.color = Color.red;
                canCreate = false; // 하나라도 부족하면 false
            }
        }

    }
}
