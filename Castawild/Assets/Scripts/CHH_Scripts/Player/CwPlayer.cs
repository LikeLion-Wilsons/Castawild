using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 테스트용
[System.Serializable]
public class HoldTool
{
    public string toolName;
    public GameObject tool;
}

// 테스트용
public enum MoveType { Idle, Walk, Run, Crouch, Jump }
public enum AttackType { None, Aim, Attack }

public class CwPlayer : NetworkBehaviour
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
    public GameObject crosshairImage;
    public float throwForce = 10f;
    public float throwUpForce = 5f;
    #endregion

    #region Tool
    [SerializeField] private Transform tools;
    private Dictionary<int, GameObject> toolDict = new Dictionary<int, GameObject>();
    private GameObject currentEquippedTool = null;
    #endregion

    public InventoryDataManager inventory;
    [HideInInspector] public bool isAimLocked = false;

    // 테스트용
    public List<HoldTool> holdTools = new List<HoldTool>();

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
            Debug.Log($"Equipped: {toolIdx}");
        }
        else
        {
            Debug.LogWarning($"Tool with ItemID '{toolIdx}' not found in the dictionary.");
        }
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
            Debug.Log("Tool unequipped.");
        }
    }
}
