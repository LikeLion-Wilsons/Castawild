using System.Collections.Generic;
using UnityEngine;

public class Canvas_Holder : MonoBehaviour
{
    public static Canvas_Holder instance = null;
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    [SerializeField] UIPart[] parts;
    private Dictionary<string, UIPart> uiParts = new Dictionary<string, UIPart>();
    public void OpenUI(string uiName)
    {
        if (uiParts.ContainsKey(uiName))
        {
            uiParts[uiName].Open();
        }
        else Debug.LogWarning($"UI {uiName} not found.");
    }

    public void CloseUI(string uiName)
    {
        if (uiParts.ContainsKey(uiName))
        {
            uiParts[uiName].Close();
        }
    }

    public void CloseAllUI()
    {
        foreach (var part in uiParts.Values)
        {
            part.Close();
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
        if (Input.GetKeyDown(KeyCode.I))
        {
            uiParts["Inventory"].Toggle();
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
