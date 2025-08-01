using System.Collections.Generic;
using UnityEngine;

public class Canvas_Holder : MonoBehaviour
{
    public GameObject hotBarUI;
    public GameObject inventoryUI;
    public GameObject tableUI;

    // 수정한 부분
    public Player player;

    private void Awake()
    {
    }


    [SerializeField] UIPart[] parts;
    private Dictionary<string, UIPart> uiParts = new Dictionary<string, UIPart>();
    public void OpenUI(string uiName)
    {
        if (uiParts.ContainsKey(uiName))
        {
            uiParts[uiName].Open(player.inputManager);
        }
        else Debug.LogWarning($"UI {uiName} not found.");
    }

    public void CloseUI(string uiName)
    {
        if (uiParts.ContainsKey(uiName))
        {
            uiParts[uiName].Close(player.inputManager);
        }
    }

    public void CloseAllUI()
    {
        foreach (var part in uiParts.Values)
        {
            part.Close(player.inputManager);
        }
    }

    [SerializeField] private Transform ui_Part_Parent;
    void Start()
    {
        parts = ui_Part_Parent.GetComponentsInChildren<UIPart>(true);//비활성화된 오브젝트도 찾기
        foreach (var part in parts)
        {
            uiParts.Add(part.name, part);
        }

        // 수정한 부분
        uiParts["Inventory"].Toggle();
        uiParts["Table"].Toggle();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            uiParts["Inventory"].Toggle(player.inputManager);
            uiParts["Table"].Toggle(player.inputManager);

            // 수정한 부분
            if (uiParts["Inventory"].IsOpen())
                player.RPC_IsUIOpen(true);
            else
                player.RPC_IsUIOpen(false);
        }
    }

    public bool IsInventoryOpen()
    {
        return uiParts.ContainsKey("Inventory") && uiParts["Inventory"].IsOpen();
    }

    public bool AnyUIOpen()
    {
        foreach (var part in uiParts.Values)
        {
            if (part.IsOpen())
                return true;
        }
        return false;
    }
}
