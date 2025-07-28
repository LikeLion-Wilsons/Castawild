using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITable : MonoBehaviour
{
    [SerializeField] List<Button> craftableItems = new List<Button>();
    [SerializeField] GameObject button;
    [SerializeField] Transform parent;
    public GameObject descPanel;

    void Start()
    {
        List<Item_Scriptable> itemDataList = ItemDataBase.Instance.items;
        //제작 아이템 목록
        for (int i = 0; i<itemDataList.Count; i++)
        {
            if (itemDataList[i].itemID >= 300)
            {
                GameObject go = Instantiate(button);
                go.transform.SetParent(parent);
                go.GetComponent<CraftableItemUI>().Init(descPanel, ItemDataBase.Instance.GetItemByID(itemDataList[i].itemID));
            }
        }
    }

    void Update()
    {
        
    }
}
