using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item_Panel :
    MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Item item;
    public GameObject itemData;
    public GameObject inventory;

    public Image image;
    public Color selectedColor, notSelectedColor;


    public Image item_icon;
    public TextMeshProUGUI itemCountText;
    [SerializeField] Image durabilityBar;//내구도 UI
    [SerializeField] Sprite[] icons;

    [Header("Drag&Drop")]
    [SerializeField] private Transform originalParent;
    [SerializeField] private Transform onDragParent;
    [SerializeField] private RectTransform rectTransform;
    private Vector2 originalAnchoredPos;
    private CanvasGroup canvasGroup;
    private int originalCount;

    GameObject draggedClone;

    private bool isRightMouseDrag = false;    //우클릭 드래그 플래그

    void Start()
    {
        Deselect();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }
    public void SlotInit(Item _item)
    {
        item = _item;
    }

    public void SetItemSlot()
    {
        if (item != null && item.item_Data != null && item.count != 0)
        {
            itemData.gameObject.SetActive(item.item_Data);
            //임시
            item_icon.sprite = icons[item.item_Data.itemID];
            itemCountText.text = item.count.ToString();
            durabilityBar.fillAmount = item.durability;
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
        if (item == null || item.item_Data == null) return;
        inventory.GetComponent<UIInventory>().SetItemClickAnimation(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (item == null || item.item_Data == null) return;
        if (inventory.GetComponent<UIInventory>().itemClick.activeSelf == true)
            inventory.GetComponent<UIInventory>().itemClick.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        if (item == null || item.item_Data == null) return;

        originalAnchoredPos = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        //우클릭 여부 저장
        isRightMouseDrag = Input.GetMouseButton(1);

        if (isRightMouseDrag)
        {
            int half = item.count / 2;
            originalCount = item.count;

            // 원래 슬롯에 절반 남기기
            item.count -= half;
            SetItemSlot();

            // 복제 오브젝트 생성
            draggedClone = Instantiate(gameObject, onDragParent);
            var clonePanel = draggedClone.GetComponent<Item_Panel>();

            // 복제 아이템 설정
            clonePanel.item = new Item
            {
                item_Data = item.item_Data,
                count = half,
                durability = item.durability
            };
            clonePanel.inventory = inventory;
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
                item.count = originalCount;
                SetItemSlot();
            }

            Destroy(draggedClone);
        }
        else
        {
            if (transform.parent == onDragParent)
            {
                transform.SetParent(originalParent);
                rectTransform.anchoredPosition = originalAnchoredPos;
            }
        }
        canvasGroup.blocksRaycasts = true;
        SetItemSlot();
        inventory.GetComponent<UIInventory>().SetItemList();
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventoryItem inventoryitem = eventData.pointerDrag.GetComponent<InventoryItem>();
        var droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        var droppedPanel = droppedObj.GetComponent<Item_Panel>();
        if (droppedPanel == null || droppedPanel == this) return;

        int indexA = transform.GetSiblingIndex();
        int indexB = droppedPanel.transform.GetSiblingIndex();

        var items = InventoryDataManager.Instance.itemList;
        var fromItem = items[indexB];
        var toItem = items[indexA];

        // 우클릭 드래그: 절반만 이동
        if (droppedPanel.isRightMouseDrag)
        {
            int half = droppedPanel.originalCount / 2;
            if (half <= 0) return;

            if (toItem.item_Data == null)
            {
                // 빈 슬롯이면 새 아이템 삽입
                items[indexA] = new Item
                {
                    item_Data = fromItem.item_Data,
                    count = half,
                    durability = fromItem.durability
                };
                // 원래 아이템에서 수량 차감
                toItem.count -= droppedPanel.originalCount / 2;
            }
            else
            {
                // 다른 아이템이면 무시
                return;
            }
            droppedPanel.SetItemSlot();
            inventory.GetComponent<UIInventory>().SetItemList();
            SetItemSlot();
        }
        //좌클릭 드래그
        else
        {
            if (toItem.item_Data != null)//이동하려는 슬롯이 null이 아닌 경우
            {
                if (toItem.item_Data.itemID == fromItem.item_Data.itemID && toItem.count + fromItem.count < 20
                    && fromItem.item_Data.type != Item_Type.Equipment)
                {
                    // 같은 아이템이면 합치기
                    toItem.count += fromItem.count;
                    InventoryDataManager.Instance.ThrowItem(indexB);//합쳐지는 아이템 삭제
                    inventory.GetComponent<UIInventory>().SetItemList();
                    return;
                }

            }
            inventory.GetComponent<UIInventory>().SwapItems(indexA, indexB);
        }
    }
}

