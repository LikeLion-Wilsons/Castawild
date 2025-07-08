using UnityEngine;
using System.Collections.Generic;

public class UIInventory : UIPart
{
    public Item_Panel item_panel;
    public Transform content;

    public List<Item_Panel> itemPanels = new List<Item_Panel>();
    public List<Item> items = new List<Item>(20);

    int itemMaximumValue = 20;

    public GameObject itemClick;

    private void Start()
    {
        InventoryDataManager.onNewItemAdded += AddItemToFirstEmptySlot;
        InventoryDataManager.onInventoryUpdated += RefreshUI;
        Init();
    }
    public void Init()
    {
        items.Clear();
        itemPanels.Clear();

        itemMaximumValue = content.childCount;

        //아이템 슬롯 할당
        for (int i = 0; i < itemMaximumValue; i++)
        {
            itemPanels.Add(content.GetChild(i).GetComponent<Item_Panel>());
        }
        //갖고있는 아이템 추가
        foreach (var item in InventoryDataManager.Instance.slot_List)
        {
            items.Add(item);
        }
        // 부족한 부분 null 채우기
        while (items.Count < itemMaximumValue)
        {
            items.Add(null); // 빈 슬롯 의미
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
        // 아이템 데이터 교환
        if (items[indexB] == null)
        {
            items[indexB] = items[indexA];
            items[indexA] = null;
        }
        else
        {
            var tempItem = items[indexA];
            items[indexA] = items[indexB];
            items[indexB] = tempItem;
        }


        // 패널에 바뀐 아이템 반영
        itemPanels[indexA].SlotInit(items[indexA], this);

        itemPanels[indexB].SlotInit(items[indexB], this);
        itemPanels[indexA].SetItemSlot();
        itemPanels[indexB].SetItemSlot();
    }

    //아이템을 얻을 때 실행
    public void SetItemList()
    {
        for (int i = 0; i < itemPanels.Count; i++)
        {
            if (i < items.Count)
                itemPanels[i].SlotInit(items[i], this);
            else
                itemPanels[i].SlotInit(null, this); // 빈 슬롯 처리
            itemPanels[i].SetItemSlot();//아이콘, 개수 설정
        }
    }

    public void SetItemClickAnimation(Item_Panel panel)
    {
        itemClick.gameObject.SetActive(true);
        itemClick.transform.SetParent(panel.transform);
        itemClick.transform.localPosition = Vector2.zero;
    }

    public void AddItemToFirstEmptySlot(Item newItem)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                items[i] = newItem;
                itemPanels[i].SlotInit(newItem, this);
                itemPanels[i].SetItemSlot();
                return;
            }
        }

        Debug.LogWarning("인벤토리에 빈 공간이 없습니다!");
    }
}
