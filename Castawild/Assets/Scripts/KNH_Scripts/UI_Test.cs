using UnityEngine;

public class UI_Test : MonoBehaviour
{
    [SerializeField] Item_Scriptable itemData;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            InventoryManager.GetItem(itemData, 1);
        }
    }
}
