using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Item_Scriptable item;
    public int count = 1;

    [Header("UI")]
    public Image image;
    public Text countText;

    [HideInInspector] public Transform parentAfterDrag;
    public void InitializeItem(Item_Scriptable newItem)
    {
        item = newItem;
        image.sprite = newItem.image;
        RefreshCount();
    }

    public void RefreshCount()
    {
        countText.text = count.ToString();
        bool textActive = count > 1;
        countText.gameObject.SetActive(textActive);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        //image.raycastTarget = false;
        //parentAfterDrag = transform.parent;
        //transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //image.raycastTarget = true;
        //transform.SetParent(parentAfterDrag);
    }
}
