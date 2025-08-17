using TMPro;
using UnityEngine;

public class UIEquipment : UIPart
{
    [SerializeField] Item_Panel helmetSlot;
    [SerializeField] Item_Panel armorSlot;
    [SerializeField] Item_Panel shoesSlot;

    [SerializeField] TextMeshProUGUI helmetDefense;
    [SerializeField] TextMeshProUGUI armorDefense;
    [SerializeField] TextMeshProUGUI shoesDefense;
    [SerializeField] TextMeshProUGUI totlDefense;

    public int totalDefense = 0;

    UI_Manager uiManager;

    void Start()
    {
        InventoryDataManager.onInventoryUpdated -= SetStatUI;
        InventoryDataManager.onInventoryUpdated += SetStatUI;

        uiManager = GetComponentInParent<UI_Manager>();
    }

    void Update()
    {

    }

    public void SetStatUI()
    {
        int helmetStat = 0;
        int armorStat = 0;
        int shoesStat = 0;
        if (helmetSlot.item.itemID != -1) helmetStat = helmetSlot.item.GetData().defense;
        if (armorSlot.item.itemID != -1) armorStat = armorSlot.item.GetData().defense;
        if (shoesSlot.item.itemID != -1) shoesStat = shoesSlot.item.GetData().defense;

        totalDefense = helmetStat + armorStat + shoesStat;
        helmetDefense.text = helmetStat.ToString();
        armorDefense.text = armorStat.ToString();
        shoesDefense.text = shoesStat.ToString();
        totlDefense.text = totalDefense.ToString();

        uiManager.player.playerData.defense = totalDefense;
    }
}
