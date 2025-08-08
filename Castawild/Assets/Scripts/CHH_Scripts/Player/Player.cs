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
    #region Status
    [Header("Status")]
    public PlayerData playerData = new PlayerData();

    [Header("StatusRate")]
    public float hpDecreaseRate = 0.5f;
    public float staminaIncreaseRate = 2f;
    public float staminaHungerDecreaseRate = 3f;
    public float staminaRunDecreaseRate = 1f;
    public float hungerDecreaseRate = 1f;
    public float thirstDecreaseRate = 1f;

    [Header("Current Status")]
    [Networked] public float Hp { get; set; }
    [Networked] public float Stamina { get; set; }
    [Networked] public float Hunger { get; set; }
    [Networked] public float Thirst { get; set; }
    [Networked] public float Temperature { get; set; }
    #endregion

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

    [Header("Bow")]
    [SerializeField] private Transform bowOriginalParent;
    [SerializeField] private Transform bowUseParent;
    [SerializeField] private Transform bowUseLocalParent;
    public GameObject arrow;

    [Networked, HideInInspector] public bool HasArrow { get; set; }
    private GameObject currentToolObject;
    #endregion

    #region Interact
    [Header("Interact")]
    [HideInInspector] public Bed Host_currentBed;
    #endregion

    public Coroutine fallingCoroutine;
    public GameObject amarture;

    [Header("Networked")]
    [Networked, HideInInspector] public Vector3 RespawnPos { get; set; }
    [Networked, HideInInspector] public bool CanMove { get; set; } = true;
    [Networked, HideInInspector] public bool CanPVP { get; set; } = true;
    [Networked, HideInInspector] public bool IsUIOpen { get; set; }
    [Networked, HideInInspector] public bool IsCursorLocked { get; set; }
    [Networked, HideInInspector] public bool IsSleeping { get; set; }
    [Networked] public string CurrentToolName { get; set; }
    [Networked, HideInInspector] public int CurrentToolAtt { get; set; }
    [Networked, HideInInspector] public int CurrentToolID { get; set; }

    [HideInInspector] public InventoryDataManager inventory;
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
        if (movementManager.CurrentMoveState == MovementState.Death)
            return;

        if (HasStateAuthority)
        {
            if (Thirst <= 0)
                Stamina -= staminaHungerDecreaseRate * Runner.DeltaTime;
            else
                Thirst -= thirstDecreaseRate * Runner.DeltaTime;

            if (Hunger <= 0)
            {
                TakeDamage(false, hpDecreaseRate * Runner.DeltaTime);
                Stamina -= staminaHungerDecreaseRate * Runner.DeltaTime;
            }
            else
                Hunger -= hungerDecreaseRate * Runner.DeltaTime;

            if (toolStateManager.All_CanRecoverStamina() && movementManager.All_CanRecoverStamina())
            {
                if (Stamina < playerData.maxStamina)
                    Stamina += staminaIncreaseRate * Runner.DeltaTime;
                else
                    Stamina = playerData.maxStamina;
            }
        }
    }

    /// <summary>
    /// 스폰할 때 초기화
    /// </summary>
    public void Init()
    {
        RespawnPos = transform.position;
    }

    /// <summary>
    /// 도구 장착
    /// </summary>
    public void Client_ApplySelectedItem(int itemIdx)
    {
        RPC_NotifySetCurrentItemType(itemIdx);

        // 도구일 경우 장착
        if (currentItemType == ItemType.Tool)
        {
            if (toolDict.TryGetValue(itemIdx, out GameObject currentToolGameObject))
            {
                if (HasInputAuthority)
                    RPC_NotifyEquipmentTool(itemIdx);
                RPC_RequestSetCurrentTool(currentToolGameObject.GetComponent<ToolInfo>());
            }
            else
                Debug.LogWarning($"{itemIdx} 인덱스 없음");
        }
        else
        {
            RPC_NotifyEquipmentTool();
            RPC_RequestSetCurrentTool();
        }

        toolStateManager.RPC_NotifyChangeSelectedItem(itemIdx);
    }

    /// <summary>
    /// 도구 해제
    /// </summary>
    public void Client_RemoveSelectedItem()
    {
        RPC_NotifyEquipmentTool();
        RPC_RequestSetCurrentTool();

        toolStateManager.RPC_NotifyChangeSelectedItem();
    }

    /// <summary>
    /// 도구 사용 가능한지
    /// </summary>
    public bool All_CanUseTool() => !IsUIOpen && CanMove;

    /// <summary>
    /// 현재 들고있는 도구 + 플레이어 공격력
    /// </summary>
    public int All_GetToolAtt(string toolName = "")
    {
        if (CurrentToolName == string.Empty)
            return playerData.attack;

        if (CurrentToolName.Contains(toolName))
            return playerData.attack + CurrentToolAtt;
        else if (CurrentToolID > 400 || CurrentToolID == 202)
            return playerData.attack + 2;
        else
            return playerData.attack;
    }

    /// <summary>
    /// 수면 끝나고 일어나는 애니메이션 이후 호출하는 함수
    /// </summary>
    public void All_FinishSleep()
    {
        if (HasInputAuthority)
            cameraManager.AttachCameraToHead(false);

        if (HasStateAuthority)
        {
            Host_currentBed.CanSleep = true;
            Host_currentBed = default;
        }
    }

    public bool All_CanMoving() => CanMove && IsCursorLocked;

    public void Client_SetCursorLocked(bool isLocked)
    {
        if (HasInputAuthority)
            RPC_RequestCursorLocked(isLocked);
    }

    public void All_ActiveArrow(bool visible)
    {
        if (HasArrow && visible)
            arrow.SetActive(visible);
        else
            arrow.SetActive(false);
    }

    public void All_SetInitBowPos(bool isBowUse)
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

    public void Client_AttachToCamera(bool attach)
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


    public void All_CurrentToolActive(bool active) => currentToolObject?.SetActive(active);

    public void SetRespawnPos(Vector3 respawnPos)
    {
        if (HasInputAuthority)
            RPC_SetRespawnPos(respawnPos);
    }

    public void Revived()
    {
        Hp = playerData.maxHp * 0.2f;
        Stamina = playerData.maxStamina;
        Thirst = playerData.maxThirst * 0.2f;
        Hunger = playerData.maxHunger * 0.2f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority)
            return;

        if (CanPVP && other.TryGetComponent<AttackObject>(out AttackObject attackObject))
        {
            if (attackObject.canAttack)
            {
                TakeDamage(true, attackObject.att);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!HasStateAuthority)
            return;

        if (CanPVP && collision.gameObject.TryGetComponent<ThrowObject>(out ThrowObject throwObject))
        {
            if (throwObject.canAttack)
            {
                TakeDamage(true, throwObject.att);
                throwObject.canAttack = false;
            }
        }
    }

    /// <summary>
    /// 플레이어 공격을 받았을 때 호출
    /// </summary>
    public void TakeDamage(bool isAttack, float att)
    {
        if (!HasStateAuthority || Hp <= 0)
            return;
        Hp -= att;

        if (Hp <= 0)
        {
            movementManager.Host_ChangeState(MovementState.Death);
            toolStateManager.Host_ChangeState(ToolState.Idle);
        }
        else if (Hp > 0 && isAttack)
        {
            movementManager.Host_ChangeState(MovementState.GetHit);
            toolStateManager.Host_ChangeState(ToolState.Idle);
        }
    }

    // 죽었는지 확인 
    public bool IsDeath() => movementManager.CurrentMoveState == MovementState.Death;

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_NotifySetCurrentItemType(int _currentItemIdx)
    {
        if (_currentItemIdx == 202)
            currentItemType = ItemType.Tool;
        // 50 ~ 59 : Drink
        else if (_currentItemIdx >= 50 && _currentItemIdx < 60)
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

    private void RPC_RequestSetCurrentTool(ToolInfo toolInfo = null)
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
    private void RPC_NotifyEquipmentTool(int itemIdx = -1)
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

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestSetUIOpen(bool isOpen) => IsUIOpen = isOpen;

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestCursorLocked(bool isLocked) => IsCursorLocked = isLocked;

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ApplyAttachCameraToHead(bool attachCamera) => cameraManager.AttachCameraToHead(attachCamera);

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ApplyTurnOffUI() => playerInteractUI.TurnOffUI();

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotifyHasArrow(bool hasArrow) => HasArrow = hasArrow;

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetRespawnPos(Vector3 respawnPos) { RespawnPos = respawnPos; }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Heal()
    {
        Hp = playerData.maxHp;
        Stamina = playerData.maxStamina;
        Thirst = playerData.maxThirst;
        Hunger = playerData.maxHunger;
        Temperature = playerData.maxTemperature;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ActiveAimUI(bool isAiming) => playerInteractUI.Aim(isAiming);

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_CanSleep_Bed(Bed bed, bool canSleep) => bed.CanSleep = canSleep;

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_CurrentBed(Bed bed) => Host_currentBed = bed;
}
