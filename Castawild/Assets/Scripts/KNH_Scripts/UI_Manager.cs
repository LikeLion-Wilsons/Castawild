using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Manager : NetworkBehaviour
{
    public GameObject hotBarUI;
    public GameObject inventoryUI;
    public GameObject tableUI;
    public GameObject chestUI;
    public GameObject campfireUI;
    public GameObject EquipmentUI;

    public bool isDragging = false;
    // 수정한 부분
    public Player player;
    public UIStats uiStats;
    [Header("상호작용중인 오브젝트")]
    public ChestDataManager currentOpenedChest;
    public GameObject currentCampFire;
    public PlayerRef playerRef;

    [SerializeField] Image craftTab;
    [SerializeField] Image equipmentTab;

    public void SetOpenedChest(ChestDataManager chest)
    {
        currentOpenedChest = chest;
    }

    [SerializeField] UIPart[] parts;
    public Dictionary<string, UIPart> uiParts = new Dictionary<string, UIPart>();
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
            if (part.name == "HotBar") continue;
            part.Close();
        }
        if (currentOpenedChest != null)
            currentOpenedChest.GetComponent<Chest>().FinishInteract();
        if (currentCampFire != null) currentCampFire.GetComponent<Campfire>().FinishInteract();
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
        CloseAllUI();
    }
    public static event Action<bool> OnUIActive;
    void Update()
    {
        // 수정한 부분
        if (uiParts["Inventory"].IsOpen())
            player.RPC_RequestSetUIOpen(true);
        else
            player.RPC_RequestSetUIOpen(false);

        if (Input.GetKeyDown(KeyCode.Tab) && isDragging == false)
        {
            if (uiParts["Inventory"].IsOpen())
            {
                CloseAllUI();
            }
            else
            {
                uiParts["Inventory"].Toggle();
                uiParts["Table"].Toggle();
                uiParts["Tabs"].Toggle();

                SoundManager.Instance.PlayLocal2D(Sound.Env_InvenOpen);
            }
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
            if (part.name == "HotBar")
                continue;
            if (part.IsOpen())
                return true;
        }
        return false;
    }

    public void SetPlayer(Player _player)
    {
        player = _player;
        uiStats.player = player;
        player.uiStats = uiStats;
    }

    public void CraftTab()
    {
        SoundManager.Instance.PlayLocal2D(Sound.UI_ButtonClick);

        Color c1 = craftTab.color;
        c1.a = 1f;

        Color c2 = equipmentTab.color;
        c2.a = 0.5f;

        CloseAllUI();
        uiParts["Table"].Open();
        uiParts["Inventory"].Open();
        uiParts["Tabs"].Open();
    }

    public void EquipmentTab()
    {
        SoundManager.Instance.PlayLocal2D(Sound.UI_ButtonClick);

        Color c1 = craftTab.color;
        c1.a = 0.5f;

        Color c2 = equipmentTab.color;
        c2.a = 1f;


        CloseAllUI();
        uiParts["Equipment"].Open();
        uiParts["Inventory"].Open();
        uiParts["Tabs"].Open();
    }
}
