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

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            uiParts["Inventory"].Toggle(player.inputManager);
            uiParts["Table"].Toggle(player.inputManager);

        }
    }

    public bool IsInventoryOpen()
    {
        return uiParts.ContainsKey("Inventory") && uiParts["Inventory"].IsOpen();
    }

    // 수정한 부분
    // 이걸로 이동이나 도구사용할 수 있는지 판단하고있어서
    // 나중에 esc UI 추가되면 여기에도 내용 추가 부탁드립니다 !
    public bool IsInventoryTableOpen()
    {
        return uiParts.ContainsKey("Inventory") && uiParts["Inventory"].IsOpen() || uiParts.ContainsKey("Table") && uiParts["Table"].IsOpen();
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
