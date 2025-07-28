using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftableItemUI : MonoBehaviour
{
    Item_Scriptable item;
    [SerializeField] Image icon;
    GameObject descPanel;
    public GameObject itemName;
    public GameObject itemDesc;

    public void Awake()
    {


    }
    public void Init(GameObject panel, Item_Scriptable _item)
    {
        descPanel = panel;
        item = _item;
        icon.sprite = item.image;

        itemName = descPanel.transform.GetChild(0).gameObject;
        itemDesc = descPanel.transform.GetChild(1).gameObject;
    }

    public void SetTableUI()
    {
        itemName.GetComponent<TextMeshProUGUI>().text = item.itemName;
        itemDesc.GetComponent<TextMeshProUGUI>().text = item.description;
    }
}
