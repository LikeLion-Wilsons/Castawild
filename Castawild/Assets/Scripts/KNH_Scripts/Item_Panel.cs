using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Item_Panel : MonoBehaviour
{
    public Item item;
    public Image item_icon;
    public TextMeshProUGUI ItemCountText;
    public float durability;//내구도

    public void Init(Item _item)
    {
        item = _item;
        //item_icon.sprite = ;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
