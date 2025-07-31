using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 테스트용
public enum MoveType { Idle, Walk, Run, Crouch, Jump }
public enum AttackType { None, Aim, Attack }
public enum ItemType { None, Tool, Food, Drink, Placeable }

public class Player : NetworkBehaviour
{
    public PlayerData playerData;

    #region Components
    [HideInInspector] public Animator anim;
    [HideInInspector] public Rigidbody rigid;
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
    private GameObject currentEquippedTool = null;
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

    [Networked, HideInInspector] public bool CanAct { get; set; } = true;

    [HideInInspector] public InventoryDataManager inventory;
    [HideInInspector] public bool isAimLocked = false;

    [HideInInspector] public ItemType currentItemType;

    private void Awake()
    {
        InitComponents();
        InitTools();
    }

    private void InitComponents()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
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
            ItemInfo itemInfo = tool.GetComponent<ItemInfo>();
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
            if (currentEquippedTool != null)
            {
                currentEquippedTool.SetActive(false);
                currentEquippedTool = null;
            }

            if (toolDict.TryGetValue(itemIdx, out GameObject newToolGameObject))
            {
                newToolGameObject.SetActive(true);
                currentEquippedTool = newToolGameObject;
            }
            else
                Debug.LogWarning($"{itemIdx} 인덱스 없음");
        }

        toolStateManager.ChangeSelectedItem(itemIdx);
    }

    private void SetCurrentItemType(int _currentItemIdx)
    {
        if (_currentItemIdx < 50)
            currentItemType = ItemType.Drink;
        else if (_currentItemIdx < 100)
            currentItemType = ItemType.Food;
        else if (_currentItemIdx >= 300 && _currentItemIdx < 400)
            currentItemType = ItemType.Placeable;
        else if (_currentItemIdx >= 400)
            currentItemType = ItemType.Tool;
    }

    /// <summary>
    /// 도구 해제
    /// </summary>
    public void RemoveSelectedItem()
    {
        if (currentEquippedTool != null)
        {
            currentEquippedTool.SetActive(false);
            currentEquippedTool = null;
        }
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

    public bool IsInventoryTableOpen()
    {
        if (inventory == null || inventory.canvasHolder == null)
            return false;
        return inventory.canvasHolder.IsInventoryTableOpen();
    }

    public bool CanUseTool()
    {
        if (inventory == null || inventory.canvasHolder == null)
            return false;
        return !inventory.canvasHolder.IsInventoryTableOpen() && CanAct;
    }

    /// <summary>
    /// 현재 들고있는 도구 + 플레이어 공격력
    /// </summary>
    public int GetToolAtt(string toolName)
    {
        if (currentEquippedTool == null)
            return playerData.attack;

        ItemInfo itemInfo = currentEquippedTool.GetComponent<ItemInfo>();

        if (itemInfo.ToolName.Contains(toolName))
            return playerData.attack + itemInfo.Att;
        else if (itemInfo.ItemID > 400)
            return playerData.attack + 2;
        else
            return playerData.attack;
    }

    public void FinishSleep()
    {
        CurrentBed.FinishSleep();
        CurrentBed = null;
        CanAct = true;
    }
}
