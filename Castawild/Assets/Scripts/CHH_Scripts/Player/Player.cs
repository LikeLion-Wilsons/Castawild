using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 테스트용
public enum MoveType { Idle, Walk, Run, Crouch, Jump }
public enum AttackType { None, Aim, Attack }
public enum ItemType { None, Default, Tool, Food, Drink, Placeable }

public class Player : NetworkBehaviour
{
    public PlayerData playerData;

    #region Components
    [HideInInspector] public Animator anim;
    [HideInInspector] public Rigidbody rigid;
    [HideInInspector] public PlayerController playerController;
    [HideInInspector] public PlayerInputManager inputManager;
    [HideInInspector] public MovementStateManager movementManager;
    [HideInInspector] public ToolStateManager toolStateManager;
    [HideInInspector] public PlayerCameraManager cameraManager;
    #endregion

    #region Throw
    public float throwForce = 10f;
    public float throwUpForce = 5f;
    #endregion

    #region Tool
    [SerializeField] private Transform tools;
    private Dictionary<int, GameObject> toolDict = new Dictionary<int, GameObject>();
    #endregion

    #region Interact
    public Image crosshairImage;
    [SerializeField] private Sprite originImage;
    [SerializeField] private Sprite axeImage;
    [SerializeField] private Sprite pickaxeImage;
    public CanvasGroup interactableUI;
    public TextMeshProUGUI interactableText;
    public CanvasGroup placeableUI;
    [Networked, HideInInspector] public Bed CurrentBed { get; set; }
    #endregion

    [Networked] public bool CanMove { get; set; } = true;
    [Networked] public bool IsUIOpen { get; set; }
    [Networked] public bool IsCursorLocked { get; set; }

    [Networked] public string CurrentToolName { get; set; }
    [Networked] public int CurrentToolAtt { get; set; }
    [Networked] public int CurrentToolID { get; set; }

    [HideInInspector] public InventoryDataManager inventory;
    [HideInInspector] public bool isAimLocked = false;
    [HideInInspector] public bool isSpawned;

    [HideInInspector] public ItemType currentItemType;

    override public void Spawned()
    {
        isSpawned = true;
        InitTools();
    }

    private void Awake()
    {
        InitComponents();
    }

    private void InitComponents()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
        inputManager = GetComponent<PlayerInputManager>();
        movementManager = GetComponent<MovementStateManager>();
        toolStateManager = GetComponent<ToolStateManager>();
        cameraManager = GetComponent<PlayerCameraManager>();
        inventory = GetComponent<InventoryDataManager>();
    }

    private void InitTools()
    {
        foreach (Transform tool in tools)
        {
            ToolInfo itemInfo = tool.GetComponent<ToolInfo>();
            if (itemInfo != null)
            {
                if (!toolDict.ContainsKey(itemInfo.ItemID))
                {
                    toolDict.Add(itemInfo.ItemID, tool.gameObject);
                    tool.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 도구 장착
    /// </summary>
    public void ApplySelectedItem(int itemIdx)
    {
        SetCurrentItemType(itemIdx);

        // 도구일 경우 장착
        if (currentItemType == ItemType.Tool)
        {
            if (toolDict.TryGetValue(itemIdx, out GameObject currentToolGameObject))
            {
                if (HasInputAuthority)
                    RPC_EquipmentTool(itemIdx);
                SetCurrentTool(currentToolGameObject.GetComponent<ToolInfo>());
            }
            else
                Debug.LogWarning($"{itemIdx} 인덱스 없음");
        }
        else
        {
            Debug.Log("Active False");
            if (HasInputAuthority)
                RPC_EquipmentTool();
            SetCurrentTool();
        }

        toolStateManager.ChangeSelectedItem(itemIdx);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_EquipmentTool(int itemIdx = -1)
    {
        foreach (var tool in toolDict)
        {
            if (tool.Value != null)
                tool.Value.SetActive(false);
        }

        if (itemIdx == -1)
            return;

        if (toolDict.TryGetValue(itemIdx, out GameObject currentToolGameObject))
            currentToolGameObject.SetActive(true);
    }

    private void SetCurrentItemType(int _currentItemIdx)
    {
        // 50 ~ 59 : Drink
        if (_currentItemIdx >= 50 && _currentItemIdx < 60)
            currentItemType = ItemType.Drink;
        // 60 ~ 69 : Food
        if (_currentItemIdx >= 60 && _currentItemIdx < 70)
            currentItemType = ItemType.Food;
        // 300 ~ 400 : Placeable
        else if (_currentItemIdx >= 300 && _currentItemIdx < 400)
            currentItemType = ItemType.Placeable;
        // 400 ~ : Tool
        else if (_currentItemIdx >= 400)
            currentItemType = ItemType.Tool;
        else
            currentItemType = ItemType.Default;
    }

    /// <summary>
    /// 도구 해제
    /// </summary>
    public void RemoveSelectedItem()
    {
        RPC_EquipmentTool();
        SetCurrentTool();

        toolStateManager.ChangeSelectedItem();
    }

    public void ChangeCrosshairUI(InteractableType type = InteractableType.None)
    {
        switch (type)
        {
            case InteractableType.Tree:
                crosshairImage.GetComponent<RectTransform>().sizeDelta = new Vector2(70f, 70f);
                crosshairImage.sprite = axeImage;
                break;
            case InteractableType.Stone:
                crosshairImage.GetComponent<RectTransform>().sizeDelta = new Vector2(70f, 70f);
                crosshairImage.sprite = pickaxeImage;
                break;
            case InteractableType.None:
            default:
                crosshairImage.GetComponent<RectTransform>().sizeDelta = new Vector2(10f, 10f);
                crosshairImage.sprite = originImage;
                break;
        }
    }

    public bool CanUseTool() => !IsUIOpen && CanMove;

    /// <summary>
    /// 현재 들고있는 도구 + 플레이어 공격력
    /// </summary>
    public int GetToolAtt(string toolName)
    {
        if (CurrentToolName == string.Empty)
            return playerData.attack;

        if (CurrentToolName.Contains(toolName))
            return playerData.attack + CurrentToolAtt;
        else if (CurrentToolID > 400)
            return playerData.attack + 2;
        else
            return playerData.attack;
    }

    public void FinishSleep()
    {
        CurrentBed.FinishSleep();
        CurrentBed = null;
        CanMove = true;
    }

    public bool CanMoving() => CanMove && IsCursorLocked;

    public void SetCursorLocked(bool isLocked)
    {
        if (HasInputAuthority)
            RPC_CursorLocked(isLocked);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_CursorLocked(bool isLocked) => IsCursorLocked = isLocked;

    public void PlayerStop()
    {
        CanMove = false;
        playerController.kcc.Move(Vector3.zero);
    }

    private void SetCurrentTool(ToolInfo toolInfo = null)
    {
        if (toolInfo == null)
        {
            CurrentToolID = -1;
            CurrentToolName = string.Empty;
            CurrentToolAtt = 0;
            return;
        }

        CurrentToolID = toolInfo.ItemID;
        CurrentToolName = toolInfo.ToolName;
        CurrentToolAtt = toolInfo.Att;
    }
}
