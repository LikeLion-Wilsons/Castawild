using TMPro;
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
    public Canvas_Holder canvasHolder;
    public InventoryDataManager inventoryData;
    public void Start()
    {
        canvasHolder = GetComponentInParent<Canvas_Holder>();
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
        inventoryData = table.inventoryData;

        //텍스트 설정
        itemName.GetComponent<TextMeshProUGUI>().text = item.itemName;
        itemDesc.GetComponent<TextMeshProUGUI>().text = item.description;

        Transform content = descPanel.transform.GetChild(0);

        // 기존 재료 아이콘 삭제
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        if (item.ingredient == null) return;//재료 없으면 무시

        table.canCreate = true;
        for (int i = 0; i<item.ingredient.Count; i++)
        {
            //재료 아이콘 추가
            GameObject go = Instantiate(ingredientPanel);
            go.transform.SetParent(content);
            go.GetComponent<Image>().sprite = item.ingredient[i].image;

            //개수 반영
            TextMeshProUGUI countText = go.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            int currentCount = inventoryData.GetItemCount(item.ingredient[i].itemID);
            int needCount = item.ingredientCount[i];
            countText.text = currentCount + "/"+ needCount.ToString();
            //텍스트 색상 설정
            if (currentCount >= needCount) countText.color = Color.white;
            else
            {
                countText.color = Color.red;
                table.canCreate = false; // 하나라도 부족하면 false
            }
        }

    }
}
