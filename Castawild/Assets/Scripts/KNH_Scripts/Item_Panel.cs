using Fusion;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Item_Panel :
    NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Item item;
    public GameObject itemData;
    public GameObject inventory;
    UIInventory uiInventory;
    InventoryDataManager inventoryData;
    public Image image;
    public Color selectedColor, notSelectedColor;

    public Image item_icon;
    public TextMeshProUGUI itemCountText;
    [SerializeField] Image durabilityBar;//내구도 UI

    [Header("Drag&Drop")]
    bool isInveontoryOpen = true;
    [SerializeField] private Transform originalParent;
    [SerializeField] private Transform onDragParent;
    [SerializeField] private RectTransform rectTransform;
    private Vector2 originalAnchoredPos;
    private CanvasGroup canvasGroup;
    private int originalCount;

    public GameObject draggedClone;
    public bool isClone = false;

    private bool isRightMouseDrag = false;    //우클릭 드래그 플래그

    void Start()
    {
        Deselect();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        uiInventory = inventory.GetComponent<UIInventory>();
        UI_Manager.OnUIActive+= OnUIActive;
    }

    void OnDestroy()
    {
        UI_Manager.OnUIActive -= OnUIActive;
    }

    void OnUIActive(bool active)
    {
        if (active == false && inventoryData.canvasHolder.isDragging)
        {
            ForceCancelDrag();
        }
    }
    public void BindToInventoryData(InventoryDataManager data)
    {
        inventoryData = data;
    }
    void Update()
    {
        if (inventoryData != null)
        {
            bool wasOpen = isInveontoryOpen;
            isInveontoryOpen = inventoryData.canvasHolder.IsInventoryOpen();
        }

    }

    //드래그 취소
    public void ForceCancelDrag()
    {
        if (isRightMouseDrag)
        {
            if (draggedClone != null)
            {
                Destroy(draggedClone);
                draggedClone = null;
            }
            item.count = originalCount;
            int index = uiInventory.GetIndex(this);
            inventoryData.RPC_SetItem(index, item);
        }
        else
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalAnchoredPos;
        }
        isRightMouseDrag = false;
        canvasGroup.blocksRaycasts = true;
        SetItemSlot();
        uiInventory.SetItemList();
    }

    public void SlotInit(Item _item)
    {
        item = _item;
    }

    public void SetItemSlot()
    {
        if (item.itemID != -1 && item.count != 0)
        {
            itemData.gameObject.SetActive(true);
            item_icon.sprite = item.GetData().image;
            //도구형 아이템
            if (item.itemID / 100 == 4)
            {
                itemCountText.gameObject.SetActive(false);//아이템 개수 표시 X
                durabilityBar.fillAmount = item.durability;//내구도 설정
            }
            else if (item.GetData().stackable == false)
            {
                itemCountText.gameObject.SetActive(false);//아이템 개수 표시 X
                durabilityBar.gameObject.SetActive(false);//내구도 UI X
            }
            else
            {
                itemCountText.text = item.count.ToString();
                durabilityBar.gameObject.SetActive(false);//내구도 UI X
            }

        }
        else
        {
            itemData.gameObject.SetActive(false);
            item_icon.sprite = null;
            itemCountText.text = "";
        }
    }

    public void Select()
    {
        image.color = selectedColor;
    }
    public void Deselect()
    {
        image.color = notSelectedColor;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item.itemID == -1) return;
        uiInventory.SetItemClickAnimation(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (item.itemID == -1) return;
        if (uiInventory.itemClick.activeSelf == true)
            uiInventory.itemClick.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isInveontoryOpen) return;

        originalAnchoredPos = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        if (item.itemID == -1) return;

        SoundManager.Instance.PlayLocal2D(Sound.UI_Scroll);

        inventoryData.canvasHolder.isDragging = true;
        //우클릭 여부 저장
        isRightMouseDrag = Input.GetMouseButton(1);

        if (isRightMouseDrag)
        {
            int half = item.count / 2;
            originalCount = item.count;

            // 원래 슬롯에 절반 남기기
            item.count -= half;
            int index = uiInventory.GetIndex(this);
            inventoryData.RPC_SetItem(uiInventory.GetIndex(this), item);
            SetItemSlot();

            // 복제 오브젝트 생성
            draggedClone = Instantiate(gameObject, onDragParent);
            var clonePanel = draggedClone.GetComponent<Item_Panel>();
            clonePanel.inventoryData = this.inventoryData;

            // 복제 아이템 설정
            clonePanel.item = new Item
            {
                itemID = item.itemID,
                count = half,
                durability = item.durability
            };
            clonePanel.inventory = inventory;
            clonePanel.isClone = true;
            clonePanel.SetItemSlot();

            // 드래그 오브젝트는 마우스 따라다니게 설정
            draggedClone.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        else
        {
            transform.SetParent(onDragParent); // 좌클릭 드래그는 기존 오브젝트 이동
        }
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isInveontoryOpen) return;
        if (item.itemID == -1) return;
        if (isRightMouseDrag && draggedClone != null)
        {
            draggedClone.GetComponent<RectTransform>().position = eventData.position;
        }
        else
        {
            rectTransform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isInveontoryOpen) return;
        inventoryData.canvasHolder.isDragging = false;

        SoundManager.Instance.PlayLocal2D(Sound.UI_ItemDrop);

        // 드래그 종료 위치 기준으로 레이캐스트
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        bool droppedOnSlot = false;
        if (isRightMouseDrag)
        {
            foreach (var result in results)
            {
                //슬롯 위에 드랍
                if (result.gameObject != gameObject && result.gameObject.GetComponent<Item_Panel>() != null)
                {
                    droppedOnSlot = true;
                    break;

                }
                else break;
            }

            //이동 실패
            if (!droppedOnSlot)
            {
                foreach (var result in results)
                {
                    //버리는 구역에 드랍했을 때
                    if (result.gameObject.GetComponent<TrashArea>() != null)
                    {
                        //버리기
                        break;
                    }
                    else
                    {
                        item.count = originalCount;
                        int index = uiInventory.GetIndex(this);
                        inventoryData.RPC_SetItem(index, item);
                        Destroy(draggedClone);
                        SetItemSlot();
                    }
                }

            }
            else
            {
                Destroy(draggedClone);
            }
        }
        else
        {
            //좌클릭 드래그: 원래 위치로
            if (transform.parent == onDragParent)
            {
                transform.SetParent(originalParent);
                rectTransform.anchoredPosition = originalAnchoredPos;
            }
        }
        canvasGroup.blocksRaycasts = true;
        SetItemSlot();
        uiInventory.SetItemList();

    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!isInveontoryOpen) return;

        var droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        var droppedPanel = droppedObj.GetComponent<Item_Panel>();
        if (droppedPanel == null || droppedPanel == this) return;

        int indexA = uiInventory.GetIndex(this);
        int indexB = uiInventory.GetIndex(droppedPanel);

        var items = inventoryData.itemList;
        var fromItem = items[indexB];
        var toItem = items[indexA];

        // 우클릭 드래그: 절반만 이동
        if (droppedPanel.isRightMouseDrag)
        {
            int half = droppedPanel.originalCount / 2;
            if (half <= 0) return;

            if (toItem.itemID == -1)
            {
                // 빈 슬롯이면 새 아이템 삽입
                items[indexA] = new Item
                {
                    itemID = fromItem.itemID,
                    count = half,
                    durability = fromItem.durability
                };
                // 원래 아이템에서 수량 차감
                //fromItem.count -= half;
                inventoryData.RPC_SetItem(indexA, items[indexA]);
                inventoryData.RPC_SetItem(indexB, fromItem);
            }
            else
            {
                // 다른 아이템이면 무시
                return;
            }
            droppedPanel.SetItemSlot();
            uiInventory.SetItemList();
            SetItemSlot();
        }
        //좌클릭 드래그
        else
        {
            if (toItem.itemID != -1)//이동하려는 슬롯이 null이 아닌 경우
            {
                if (toItem.GetData().itemID == fromItem.GetData().itemID && toItem.count + fromItem.count <= 20
                    && fromItem.GetData().stackable == true)
                {
                    // 같은 아이템이면 합치기
                    toItem.count += fromItem.count;
                    inventoryData.RPC_SetItem(indexA, toItem);
                    Item item = new Item
                    {
                        itemID = -1,
                        count = 0,
                        durability = 1
                    };
                    inventoryData.RPC_SetItem(indexB, item);//합쳐지는 아이템 삭제
                    uiInventory.SetItemList();
                    return;
                }
            }
            //위치 교환
            inventoryData.RPC_SwapItems(indexA, indexB);
        }
    }
    public bool IsEmpty()
    {
        return item.itemID == -1 || item.count <= 0;
    }
}

