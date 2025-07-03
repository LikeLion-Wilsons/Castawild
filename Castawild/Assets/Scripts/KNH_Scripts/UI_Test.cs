using UnityEngine;

public class UI_Test : MonoBehaviour
{
    [SerializeField] Item_Scriptable[] itemData;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("나뭇가지 획득");
            InventoryManager.GetItem(itemData[0], 1);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("돌 획득");
            InventoryManager.GetItem(itemData[1], 1);
        }
    }
}
