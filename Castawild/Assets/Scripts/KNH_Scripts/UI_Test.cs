using UnityEngine;

public class UI_Test : MonoBehaviour
{
    [SerializeField] Item_Scriptable[] itemData;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("나뭇가지 획득");
            InventoryDataManager.Instance.GetItem(itemData[0], 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("돌 획득");
            InventoryDataManager.Instance.GetItem(itemData[1], 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("도끼 획득");
            InventoryDataManager.Instance.GetItem(itemData[2], 1);
        }
    }
}
