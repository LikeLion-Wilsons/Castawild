using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item_Panel : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    public Item item;
    public GameObject itemPanel;
    public Image item_icon;
    public TextMeshProUGUI itemCountText;
    public float durability;//내구도
    [SerializeField] Sprite[] icons;
    public Inventory parentPanel;

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
}

