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

    public Image item_icon;
    public TextMeshProUGUI itemCountText;
    public float durability;//내구도
    [SerializeField] Sprite[] icons;

    [Header("Drag&Drop")]
    [SerializeField] private Transform originalParent;
    [SerializeField] private Transform onDragParent;
    [SerializeField] private RectTransform rectTransform;
    private Vector2 originalAnchoredPos;
    private CanvasGroup canvasGroup;

    void Start()
    {
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
        }
        else
        {
            itemData.gameObject.SetActive(false);
            item_icon.sprite = null;
            itemCountText.text = "";
        }
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
        originalAnchoredPos = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        if (item == null || item.item_Data == null) return;

        canvasGroup.blocksRaycasts = false;
        transform.SetParent(onDragParent);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item == null || item.item_Data == null) return;
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드롭 성공 못했으면 원래 위치로 복귀
        if (transform.parent == onDragParent)
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalAnchoredPos;
        }
        canvasGroup.blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        var droppedPanel = droppedObj.GetComponent<Item_Panel>();
        if (droppedPanel == null || droppedPanel == this) return;

        int indexA = transform.GetSiblingIndex();
        int indexB = droppedPanel.transform.GetSiblingIndex();

        inventory.GetComponent<UIInventory>().SwapItems(indexA, indexB);
    }
}

