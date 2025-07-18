using Fusion;
using UnityEngine;

public class UI_Test : NetworkBehaviour
{

    [SerializeField] Item_Scriptable[] itemData;
    public Item[] itemsToPickUp;



    public override void FixedUpdateNetwork()
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
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (Canvas_Holder.instance.IsInventoryOpen()) return;
            InventoryDataManager.Instance.ThrowItem(InventoryDataManager.Instance.GetSelectedIndex());
        }
    }

    public void PickUpItem(int id)
    {
        //bool result = InventoryDataManager.Instance.AddItem(itemData[id]);
        bool result = InventoryDataManager.Instance.GetItem(itemData[id], 1);
        if (result == true)
        {
            Debug.Log(itemData[id].name + " 획득");
        }
        else
        {
            Debug.Log("인벤토리가 가득찼습니다.");
        }
    }

    public void GetSelectedItem()
    {
        Item_Scriptable receivedItem = InventoryDataManager.Instance.GetSeletedItem(false);
        if (receivedItem != null)
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
}
