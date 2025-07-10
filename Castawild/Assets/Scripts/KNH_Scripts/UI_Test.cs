using UnityEngine;

public class UI_Test : MonoBehaviour
{
    
    [SerializeField] Item_Scriptable[] itemData;
    public Item[] itemsToPickUp;

    public void PickUpItem(int id)
    {
        bool result = InventoryDataManager.Instance.AddItem(itemData[id]);
        if(result == true)
        {
            Debug.Log(itemData[id].name+" 획득");
        }
        else
        {
            Debug.Log("인벤토리가 가득찼습니다.");
        }
    }

    public void GetSelectedItem()
    {
        Item_Scriptable receivedItem = InventoryDataManager.Instance.GetSeletedItem(false);
        if(receivedItem != null)
        {
            Debug.Log("Received item : " + receivedItem);
        }
        else
        {
            Debug.Log("No Item Received!");
        }
    }

    public void UseSelectedItem()
    {
        Item_Scriptable receivedItem = InventoryDataManager.Instance.GetSeletedItem(true);
        if (receivedItem != null)
        {
            Debug.Log("Used item : " + receivedItem);
        }
        else
        {
            Debug.Log("No Item Used!");
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            PickUpItem(0);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            PickUpItem(1);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            PickUpItem(2);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            UseSelectedItem();
        }

    }
}
