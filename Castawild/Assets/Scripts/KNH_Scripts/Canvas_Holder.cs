using UnityEngine;

public class Canvas_Holder : MonoBehaviour
{
    [SerializeField] private GameObject InventoryPanel;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            InventoryPanel.SetActive(!InventoryPanel.activeSelf);
        }    
    }
}
