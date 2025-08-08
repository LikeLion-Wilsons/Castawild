using UnityEngine;
using UnityEngine.UI;

public class CraftableItemUI : MonoBehaviour
{
    Item_Scriptable item;
    [SerializeField] Image icon;
    [SerializeField] GameObject ingredientPanel;
    GameObject descPanel;
    public GameObject itemName;
    public GameObject itemDesc;

    UITable table;
    public UI_Manager canvasHolder;
    public InventoryDataManager inventoryData;
    public void Start()
    {
        canvasHolder = GetComponentInParent<UI_Manager>();
        table = canvasHolder.tableUI.GetComponent<UITable>();

    }
    public void Init(GameObject panel, Item_Scriptable _item)
    {

        descPanel = panel;
        item = _item;
        icon.sprite = item.image;

        itemName = descPanel.transform.GetChild(1).gameObject;
        itemDesc = descPanel.transform.GetChild(2).gameObject;
    }

    public void SetTableUI()
    {
        table.selectedItem = item;
        table.SetTableUI();
    }
}
