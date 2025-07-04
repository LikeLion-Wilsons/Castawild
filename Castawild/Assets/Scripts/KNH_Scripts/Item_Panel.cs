using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item_Panel :
    MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Item item;
    public GameObject itemPanel;
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
    public void Init(Item _item, Inventory inventory)
    {
        item = _item;
        parentPanel = inventory;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item.item_Data != null) return;
        parentPanel.SetItemClickAnimation(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (parentPanel == null) return;
        if (parentPanel.itemClick.activeSelf == true)
            parentPanel.itemClick.SetActive(false);
    }

    public void SetItem()
    {
        itemPanel.gameObject.SetActive(item.item_Data);
        if (item.item_Data != null)
        {
            //임시
            item_icon.sprite = icons[item.item_Data.itemID];
            itemCountText.text = item.count.ToString();
        }
        else
        {
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item.item_Data == null) return;

        originalParent = transform.parent;
        originalAnchoredPos = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(onDragParent);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item.item_Data == null) return;
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
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        Item_Panel droppedPanel = droppedObj.GetComponent<Item_Panel>();
        if (droppedPanel == null || droppedPanel == this) return;

        // 아이템 데이터 교환
        Item tempItem = item;
        item = droppedPanel.item;
        droppedPanel.item = tempItem;

        // 위치 정보 보관
        RectTransform thisRT = GetComponent<RectTransform>();
        RectTransform droppedRT = droppedPanel.GetComponent<RectTransform>();

        Vector2 thisAnchoredPos = thisRT.anchoredPosition;
        Transform thisParent = transform.parent;

        Vector2 droppedAnchoredPos = droppedRT.anchoredPosition;
        Transform droppedParent = droppedPanel.transform.parent;

        // 부모 및 위치 교환
        droppedPanel.transform.SetParent(thisParent, false);
        droppedRT.anchoredPosition = thisAnchoredPos;

        transform.SetParent(droppedParent, false);
        thisRT.anchoredPosition = droppedAnchoredPos;

        // UI 갱신
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)originalParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
        SetItem();
        droppedPanel.SetItem();
        this.SetItem();

    }
}

