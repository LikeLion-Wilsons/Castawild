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
    public Inventory parentPanel;
    [Header("Drag&Drop")]
    [SerializeField] private Transform originalParent;
    [SerializeField] private Transform onDragParent;
    [SerializeField] private RectTransform rectTransform;
    private Vector2 originalAnchoredPos;
    [SerializeField] private Canvas canvas; // 부모 캔버스 (필요시 연결)
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }
    public void SlotInit(Item _item, Inventory inventory)
    {
        item = _item;
        parentPanel = inventory;
    }
    public void SetItemSlot()
    {
        if (item != null && item.item_Data != null)
        {
            itemData.gameObject.SetActive(item.item_Data);
            //임시
            item_icon.sprite = icons[item.item_Data.itemID];
            itemCountText.text = item.count.ToString();
        }
        else
        {
            itemData.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null || item.item_Data == null) return;
        inventory.GetComponent<Inventory>().SetItemClickAnimation(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (item == null || item.item_Data == null) return;
        if (inventory.GetComponent<Inventory>().itemClick.activeSelf == true)
            inventory.GetComponent<Inventory>().itemClick.SetActive(false);
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
        if (droppedPanel.item == null || droppedPanel == this) return;

        int indexA = transform.GetSiblingIndex();
        int indexB = droppedPanel.transform.GetSiblingIndex();

        inventory.GetComponent<Inventory>().SwapItems(indexA, indexB);
    }
}

