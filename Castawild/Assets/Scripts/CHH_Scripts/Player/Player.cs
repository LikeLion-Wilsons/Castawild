using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 테스트용
public enum MoveType { Idle, Walk, Run, Crouch, Jump }
public enum AttackType { None, Aim, Attack }
public enum ItemType { None, Default, Tool, Food, Drink, Placeable }

public class Player : NetworkBehaviour
{
    [Header("Status")]
    public PlayerData playerData = new PlayerData();

    [Header("Current Status")]
    [Networked] public float Hp { get; set; }
    [Networked] public float Stamina { get; set; }
    [Networked] public float Hunger { get; set; }
    [Networked] public float Thirst { get; set; }
    [Networked] public float Temperature { get; set; }

    public float staminaIncreaseRate = 2f;
    public float staminaDecreaseRate = 1f;
    public float hungerDecreaseRate = 1f;
    public float thirstDecreaseRate = 1f;

    #region Components
    [HideInInspector] public Animator anim;
    [HideInInspector] public PlayerInteractUI playerInteractUI;
    [HideInInspector] public PlayerController playerController;
    [HideInInspector] public PlayerInputManager inputManager;
    [HideInInspector] public MovementStateManager movementManager;
    [HideInInspector] public ToolStateManager toolStateManager;
    [HideInInspector] public PlayerCameraManager cameraManager;
    #endregion

    #region Tool
    [Header("Tool")]
    [SerializeField] private Transform tools;
    private Dictionary<int, GameObject> toolDict = new Dictionary<int, GameObject>();
    [SerializeField] private Transform bowOriginalParent;
    [SerializeField] private Transform bowUseParent;
    [SerializeField] private Transform bowUseLocalParent;
    public GameObject arrow;

    [Networked, HideInInspector] public bool HasArrow { get; set; }
    private GameObject currentToolObject;
    #endregion

    #region Interact
    [Header("Interact")]
    [HideInInspector] public Bed currentBed;
    #endregion

    public Coroutine fallingCoroutine;

    [Header("Networked")]
    [Networked, HideInInspector] public Vector3 RespawnPos { get; set; }
    [Networked, HideInInspector] public bool CanMove { get; set; } = true;
    [Networked, HideInInspector] public bool IsUIOpen { get; set; }
    [Networked, HideInInspector] public bool IsCursorLocked { get; set; }

    [Networked] public string CurrentToolName { get; set; }
    [Networked, HideInInspector] public int CurrentToolAtt { get; set; }
    [Networked, HideInInspector] public int CurrentToolID { get; set; }
    [Networked, HideInInspector] public bool IsSleeping { get; set; }

    [HideInInspector] public InventoryDataManager inventory;
    [HideInInspector] public bool isAimLocked = false;
    [HideInInspector] public bool isSpawned;

    [HideInInspector] public ItemType currentItemType;

    override public void Spawned()
    {
        isSpawned = true;
        InitStatus();
        InitTools();
    }

    private void InitStatus()
    {
        Hp = playerData.maxHp;
        Stamina = playerData.maxStamina;
        Hunger = playerData.maxHunger;
        Thirst = playerData.maxThirst;
        Temperature = playerData.maxTemperature;
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

    public void Init()
    {
        RespawnPos = transform.position;
    }

    private void Awake()
    {
        InitComponents();
    }

    private void InitComponents()
    {
        anim = GetComponentInChildren<Animator>();
        playerController = GetComponent<PlayerController>();
        playerInteractUI = GetComponentInChildren<PlayerInteractUI>();
        inputManager = GetComponent<PlayerInputManager>();
        movementManager = GetComponent<MovementStateManager>();
        toolStateManager = GetComponent<ToolStateManager>();
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
        inventory = GetComponent<InventoryDataManager>();
    }

    public override void FixedUpdateNetwork()
    {
        if (movementManager.currentState == movementManager.deathState)
            return;

        if (HasStateAuthority)
        {
            Hunger -= hungerDecreaseRate * Runner.DeltaTime;
            Thirst -= thirstDecreaseRate * Runner.DeltaTime;

            if (toolStateManager.CanRecoverStamina() && movementManager.CanRecoverStamina())
            {
                if (Stamina < playerData.maxStamina)
                    Stamina += staminaIncreaseRate * Runner.DeltaTime;
                else
                    Stamina = playerData.maxStamina;
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
            if (HasInputAuthority)
                RPC_EquipmentTool();
            SetCurrentTool();
        }

        if (HasInputAuthority)
            toolStateManager.RPC_ChangeSelectedItem(itemIdx);
    }

    private void SetCurrentItemType(int _currentItemIdx)
    {
        // 50 ~ 59 : Drink
        if (_currentItemIdx >= 50 && _currentItemIdx < 60)
            currentItemType = ItemType.Drink;
        // 60 ~ 69 : Food
        else if (_currentItemIdx >= 60 && _currentItemIdx < 70)
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

        toolStateManager.RPC_ChangeSelectedItem();
    }

    public bool CanUseTool()
    {
        return !IsUIOpen && CanMove;
    }

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
        if (HasInputAuthority)
        {
            currentBed.FinishSleep();
            currentBed = default;
            cameraManager.AttachCameraToHead(false);
        }
    }

    public bool CanMoving() => CanMove && IsCursorLocked;

    public void SetCursorLocked(bool isLocked)
    {
        if (HasInputAuthority)
            RPC_CursorLocked(isLocked);
    }

    public void StopPlayer()
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
        {
            currentToolGameObject.SetActive(true);
            currentToolObject = currentToolGameObject;
        }
        else
        {
            currentToolObject = null;
        }
    }

    public void PlayerCanMove() => CanMove = true;

    public GameObject amarture;

    // false : 공격 끝났을 때, 조준 끝났을 때
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ActiveArrow(bool visible)
    {
        if (HasArrow && visible)
            arrow.SetActive(visible);
        else
            arrow.SetActive(false);
    }

    public void AttachToCamera(bool attach)
    {
        if (attach && cameraManager.currentView == ViewType.FirstPerson)
        {
            amarture.transform.SetParent(cameraManager.firstPersonCam.transform);
            amarture.transform.localPosition = new Vector3(0f, -3f, 0f);
            amarture.transform.localRotation = Quaternion.identity;
        }
        else
        {
            amarture.transform.SetParent(transform);
            amarture.transform.localPosition = Vector3.zero;
            amarture.transform.localRotation = Quaternion.identity;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_IsUIOpen(bool isOpen) => IsUIOpen = isOpen;

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_CursorLocked(bool isLocked) => IsCursorLocked = isLocked;

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_AttachCameraToHead(bool attachCamera) => cameraManager.AttachCameraToHead(attachCamera);

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_TurnOffUI() => playerInteractUI.TurnOffUI();

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetBowPos(bool isBowUse)
    {
        if (currentToolObject == null)
            return;

        if (HasInputAuthority)
        {
            if (isBowUse && cameraManager.currentView == ViewType.FirstPerson)
                currentToolObject.transform.SetParent(bowUseLocalParent);
            else if (isBowUse && cameraManager.currentView == ViewType.ThirdPerson)
                currentToolObject.transform.SetParent(bowUseParent);
            else
                currentToolObject.transform.SetParent(bowOriginalParent);
        }
        else
        {
            if (isBowUse)
                currentToolObject.transform.SetParent(bowUseParent);
            if (!isBowUse)
                currentToolObject.transform.SetParent(bowOriginalParent);
        }

        currentToolObject.transform.localPosition = Vector3.zero;
        currentToolObject.transform.localRotation = Quaternion.identity;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_HasArrow(bool hasArrow) => HasArrow = hasArrow;

    public void CurrentToolActive(bool active) => currentToolObject?.SetActive(active);

    public void SetRespawnPos(Vector3 respawnPos)
    {
        if (HasInputAuthority)
            RPC_SetRespawnPos(respawnPos);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetRespawnPos(Vector3 respawnPos) { RespawnPos = respawnPos; }

    /// <summary>
    /// 플레이어 공격을 받았을 때 호출
    /// </summary>
    public void AttackPlayer(float att)
    {
        if (!HasStateAuthority || Hp <= 0)
            return;
        Hp -= att;

        if (Hp <= 0)
        {
            Debug.Log("Death");
            movementManager.ChangeState(movementManager.deathState);
            toolStateManager.ChangeState(toolStateManager.idleState);
        }
        else
        {
            Debug.Log("Hit");
            movementManager.ChangeState(movementManager.getHitState);
            toolStateManager.ChangeState(toolStateManager.idleState);
        }
    }

    public void Revived()
    {
        Hp = playerData.maxHp * 0.2f;
        Stamina = playerData.maxStamina;
        Thirst = playerData.maxThirst * 0.2f;
        Hunger = playerData.maxHunger * 0.2f;
    }
}
