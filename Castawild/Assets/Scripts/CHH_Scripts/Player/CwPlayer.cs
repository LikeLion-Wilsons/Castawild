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
public enum ToolType { None, Fist, Throw, Spear, Sword, Bow, Axe, Pickaxe, Knife }
public enum MoveType { Idle, Walk, Run, Crouch, Jump }
public enum AttackType { None, Aim, Attack }

public class CwPlayer : MonoBehaviour
{
    // 나중에 HideInInspector로 바꾸기
    /*[HideInInspector] */
    public Animator anim;
    [HideInInspector] public Rigidbody rigid;
    [HideInInspector] public PlayerInputManager inputManager;
    [HideInInspector] public MovementStateManager movementManager;
    [HideInInspector] public ToolStateManager toolStateManager;
    [HideInInspector] public PlayerCameraManager cameraManager;

    // 테스트용
    public List<HoldTool> holdTools = new List<HoldTool>();
    public Dictionary<ToolType, Tool> tools;
    public ToolType currentToolType;
    public MoveType currentMoveType;

    public GameObject crosshairImage;
    public float throwForce = 10f;
    public float throwUpForce = 5f;

    [HideInInspector] public bool isAimLocked = false;

    public PlayerData playerData;

    static public CwPlayer instance;

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
    }

    private void InitTools()
    {
        tools = new Dictionary<ToolType, Tool>
        {
            { ToolType.Fist, new Fist(this) },
            { ToolType.Throw, new Throw(this) },
            { ToolType.Spear, new Spear(this) },
            { ToolType.Sword, new Sword(this) },
            { ToolType.Bow, new Bow(this) },
            { ToolType.Axe, new Axe(this) },
            { ToolType.Pickaxe, new Pickaxe(this) },
            { ToolType.Knife, new Knife(this) }
        };
    }

    /// <summary>
    /// 공격시 호출
    /// </summary>
    public void ApplyTool()
    {
        if (tools.TryGetValue(currentToolType, out Tool currentTool))
            currentTool.ApplyTool();
    }

    // 테스트용
    private void OnValidate()
    {
        foreach (var holdTool in holdTools)
        {
            if (holdTool.tool != null)
                holdTool.tool.SetActive(false);
        }

        GameObject toolObject = GetHoldToolObject();
        toolObject?.SetActive(true);
    }

    /// <summary>
    /// 들고있는 도구 가져오기
    /// </summary>
    public GameObject GetHoldToolObject()
    {
        string key = currentToolType.ToString();
        return holdTools.FirstOrDefault(t => t.toolName == key)?.tool;
    }
}
