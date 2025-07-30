using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 테스트용
[System.Serializable]
public class HoldTool
{
    public string toolName;
    public GameObject tool;
}

[System.Serializable]
public enum InteractableType { None, Tree, Stone, Box, Campfire }

// 테스트용
public enum MoveType { Idle, Walk, Run, Crouch, Jump }
public enum AttackType { None, Aim, Attack }

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

    #region UI
    public Image crosshairImage;
    [SerializeField] private Sprite originImage;
    [SerializeField] private Sprite axeImage;
    [SerializeField] private Sprite pickaxeImage;
    #endregion

    [HideInInspector] public InventoryDataManager inventory;
    [HideInInspector] public bool isAimLocked = false;

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
                if (!toolDict.ContainsKey(itemInfo.itemID))
                {
                    toolDict.Add(itemInfo.itemID, tool.gameObject);
                    tool.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 도구 장착
    /// </summary>
    public void EquipTool(int toolIdx)
    {
        // 장비 인덱스 아니면 리턴
        if (toolIdx < 400)
            return;

        if (currentEquippedTool != null)
        {
            currentEquippedTool.SetActive(false);
            currentEquippedTool = null;
        }

        if (toolDict.TryGetValue(toolIdx, out GameObject newToolGameObject))
        {
            newToolGameObject.SetActive(true);
            currentEquippedTool = newToolGameObject;
        }
        else
            Debug.LogWarning($"{toolIdx} 인덱스 없음");

        toolStateManager.ChangeCurrentTool(toolIdx);
    }

    /// <summary>
    /// 도구 해제
    /// </summary>
    public void UnequipCurrentTool()
    {
        if (currentEquippedTool != null)
        {
            currentEquippedTool.SetActive(false);
            currentEquippedTool = null;
        }
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

    public bool IsInventoryTableOpen() => inventory.canvasHolder.IsInventoryTableOpen();
}
