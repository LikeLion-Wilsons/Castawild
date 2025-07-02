using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Item_Panel : MonoBehaviour
{
    public Item item;
    public GameObject itemPanel;
    public Image item_icon;
    public TextMeshProUGUI itemCountText;
    public float durability;//내구도
    [SerializeField] Sprite[] icons;

    public void Init(Item _item)
    {
        item = _item;
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
