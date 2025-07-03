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
    public Image item_icon;
    public TextMeshProUGUI itemCountText;
    public float durability;//내구도
    [SerializeField] Sprite[] icons;
    public Inventory parentPanel;
    [Header("Drag&Drop")]
    private Vector3 originPos;
    [SerializeField] private Transform originalParent;
    [SerializeField] private Transform onDragParent;
    [SerializeField] private Canvas canvas; // 부모 캔버스 (필요시 연결)
    [SerializeField] LayoutElement layoutElement;
    private CanvasGroup canvasGroup;

    void Start()
    {
        layoutElement = GetComponent<LayoutElement>();
        canvasGroup = GetComponent<CanvasGroup>();
    }
    public void Init(Item _item, Inventory inventory)
    {
        item = _item;
        parentPanel = inventory;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (parentPanel == null) return;
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

        originPos = transform.position;
        originalParent = transform.parent;

        canvasGroup.blocksRaycasts = false;
        transform.SetParent(onDragParent);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item.item_Data == null) return;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드롭 성공 못했으면 원래 위치로 복귀
        if (transform.parent == onDragParent)
        {
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero; // 제자리 복귀 (원래 위치)
            //transform.position = originPos;
        }

        canvasGroup.blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        Item_Panel droppedPanel = droppedObj.GetComponent<Item_Panel>();
        if (droppedPanel == null || droppedPanel == this) return;

        // 서로 아이템 데이터 교환
        Item tempItem = item;
        item = droppedPanel.item;
        droppedPanel.item = tempItem;

        // 부모 슬롯 교환
        Transform tempParent = droppedPanel.transform.parent;
        droppedPanel.transform.SetParent(transform.parent);
        droppedPanel.transform.localPosition = Vector3.zero;

        transform.SetParent(tempParent);
        transform.localPosition = Vector3.zero;

        // UI 갱신
        SetItem();
        parentPanel.GetComponent<Inventory>().SetItemList();
        parentPanel.GetComponent<Inventory>().SetInventory();
        droppedPanel.SetItem();
    }
}

