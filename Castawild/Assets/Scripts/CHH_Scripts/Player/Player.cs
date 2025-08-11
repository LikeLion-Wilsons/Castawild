using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using Test;
using UnityEngine;
using UnityEngine.Rendering;

// 테스트용
public enum MoveType { Idle, Walk, Run, Crouch, Jump }
public enum AttackType { None, Aim, Attack }
public enum ItemType { None, Default, Tool, Food, Drink, Placeable }

public class Player : NetworkBehaviour
{
    #region Status
    [Header("NickName")]
    [SerializeField] private NicknameUI nicknameUI;
    [Networked, OnChangedRender(nameof(All_OnChangedNickname))] private string nickname { get; set; }

    [Header("Status")]
    public PlayerData playerData = new PlayerData();

    [Header("StatusRate")]
    public float hpDecreaseRate = 0.5f;
    public float staminaIncreaseRate = 2f;
    public float staminaHungerDecreaseRate = 3f;
    public float staminaRunDecreaseRate = 1f;
    public float hungerDecreaseRate = 1f;
    public float thirstDecreaseRate = 1f;
    public float thirstSleepDecrease = 10f;
    public float hungerSleepDecrease = 10f;
    [SerializeField] private float bloodEffectThreshold = 0.2f;

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
    [Networked, HideInInspector] public bool HasPebble { get; set; }
    private GameObject currentToolObject;
    #endregion

    #region Interact
    [Header("Interact")]
    [HideInInspector] public Bed Host_currentBed;
    #endregion

    [Header("Effect")]
    [SerializeField] private Volume takeDamageEffect;
    [SerializeField] private Animator takeDamageEffectAnim;

    public Coroutine fallingCoroutine;
    public GameObject amarture;

    [Header("Networked")]
    [Networked, HideInInspector] public Vector3 RespawnPos { get; set; }
    [Networked] public bool CanMove { get; set; } = true;
    [Networked, HideInInspector] public bool CanPVP { get; set; } = true;
    [Networked, HideInInspector] public bool IsUIOpen { get; set; }
    [Networked, HideInInspector] public bool IsCursorLocked { get; set; }
    [Networked, HideInInspector] public bool IsSleeping { get; set; }
    [Networked] public string CurrentToolName { get; set; }
    [Networked, HideInInspector] public int CurrentToolAtt { get; set; }
    [Networked, HideInInspector] public int CurrentToolID { get; set; }

    [HideInInspector] public InventoryDataManager inventory;
    [HideInInspector] public bool isSpawned;
    public ItemType currentItemType;
    public event Action<int> Hit;

    private void Awake()
    {
        InitComponents();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            Host_TakeDamage(true, 10);
    }

    override public void Spawned()
    {
        isSpawned = true;
        InitStatus();
        InitTools();

        if (HasInputAuthority)
            RPC_RequestSetNickname(PlayerTempData.nickname);
        All_OnChangedNickname();
        if (HasInputAuthority)
            nicknameUI.gameObject.SetActive(false);
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
                Host_TakeDamage(false, hpDecreaseRate * Runner.DeltaTime);
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

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority)
            return;

        // 플레이어 공격
        if (CanPVP && other.TryGetComponent<AttackObject>(out AttackObject attackObject))
        {
            if (other.GetComponent<ThrowObject>() != null)
                return;

            Host_TakeDamage(true, attackObject.Att);

            attackObject.player.RPC_ApplyHitInvoke(attackObject.Att);
        }

        // 동물 공격
        //if (other.CompareTag("AnimalAttack"))
        //{

        //}
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!HasStateAuthority)
            return;

        if (CanPVP && collision.gameObject.TryGetComponent<ThrowObject>(out ThrowObject throwObject))
        {
            if (throwObject.canAttack)
            {
                Host_TakeDamage(true, throwObject.Att);
                throwObject.canAttack = false;
                throwObject.player.GetComponent<PlayerController>().RPC_ApplyHitInvoke(throwObject.Att);
            }
        }
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
                ToolInfo toolInfo = currentToolGameObject.GetComponent<ToolInfo>();
                RPC_RequestSetCurrentTool(toolInfo.ItemID, toolInfo.ToolName, toolInfo.Att);
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
    /// 수면 끝나고 호출하는 함수
    /// </summary>
    public void Host_FinishSleep()
    {
        if (HasStateAuthority)
        {
            Host_currentBed.CanSleep = true;
            Host_currentBed = default;
        }
    }

    /// <summary>
    /// 움직일 수 있는지 확인
    /// </summary>
    public bool All_CanMoving() => CanMove && IsCursorLocked;

    /// <summary>
    /// 커서 잠구기
    /// </summary>
    public void Client_SetCursorLocked(bool isLocked)
    {
        if (HasInputAuthority)
            RPC_RequestCursorLocked(isLocked);
    }

    /// <summary>
    /// 화살 위치 설정
    /// </summary>
    public void All_SetBowPos(bool isBowUse)
    {
        if (currentToolObject == null)
            return;

        Debug.Log("Set Bow Pos" + isBowUse);
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

    /// <summary>
    /// 화살 활성화
    /// </summary>
    public void All_SetArrowActive(bool isBowUse)
    {
        if (HasArrow && isBowUse)
            arrow.SetActive(isBowUse);
        else
            arrow.SetActive(false);
    }

    /// <summary>
    /// 매쉬 카메라에 붙이기
    /// </summary>
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

    /// <summary>
    /// 현재 도구 활성화
    /// </summary>
    public void All_SetPebbleActive(bool active)
    {
        if (HasPebble && active)
            currentToolObject?.SetActive(true);
        else if (!active)
            currentToolObject?.SetActive(false);
    }

    /// <summary>
    /// 리스폰 위치 설정
    /// </summary>
    public void Client_SetRespawnPos(Vector3 respawnPos)
    {
        if (HasInputAuthority)
            RPC_RequestSetRespawnPos(respawnPos);
    }

    /// <summary>
    /// 부활 스테이터스
    /// </summary>
    public void Host_RevivedStatus()
    {
        Hp = playerData.maxHp * 0.2f;
        Stamina = playerData.maxStamina;
        Thirst = playerData.maxThirst * 0.2f;
        Hunger = playerData.maxHunger * 0.2f;
    }

    /// <summary>
    /// 플레이어 공격을 받았을 때 호출
    /// </summary>
    public void Host_TakeDamage(bool isAttack, float att)
    {
        if (!HasStateAuthority || Hp <= 0)
            return;
        Hp -= att;

        if (Hp <= 0)
        {
            movementManager.Host_ChangeState(MovementState.Death);
            toolStateManager.Host_ChangeState(ToolState.Idle);
            return;
        }
        else if (Hp > 0 && isAttack)
        {
            movementManager.Host_ChangeState(MovementState.GetHit);
            toolStateManager.Host_ChangeState(ToolState.Idle);

            if (isAttack)
            {
                RPC_ApplyPlayDamagedEffectAnim();
                RPC_NotifyPlayDamagedAnim();
                if (movementManager.input.currentView == ViewType.FirstPerson)
                    playerController.RPC_ApplyShakeCamera(transform.right, 0.5f);
                else
                    playerController.RPC_ApplyShakeCamera(transform.right, 0.3f);
            }
        }

        if (Hp <= playerData.maxHp * 0.2f)
            RPC_ApplyPlayDamagedEffect();
    }

    /// <summary>
    /// 죽었는지 확인
    /// </summary>
    public bool All_IsDead() => movementManager.CurrentMoveState == MovementState.Death || Hp <= 0;

    public void Host_InitCurrentTool()
    {
        CurrentToolID = -1;
        CurrentToolName = string.Empty;
        CurrentToolAtt = 0;

        RPC_NotifyInitCurrentToolObject();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ApplyHitInvoke(int dmg) => Hit?.Invoke(dmg);

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_ApplyPlayDamagedEffect()
    {
        float hpPercent = Hp / playerData.maxHp;

        if (hpPercent <= bloodEffectThreshold)
            takeDamageEffect.weight = Mathf.InverseLerp(bloodEffectThreshold, 0f, hpPercent);
        Debug.Log("takeDamageEffect Weight : " + takeDamageEffect.weight);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_ApplyPlayDamagedEffectAnim() => takeDamageEffectAnim.SetTrigger("Damaged");

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotifyPlayDamagedAnim() => anim.SetTrigger("GetHit");

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotifyArrowActive(bool isActive) => arrow.SetActive(false);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotifyInitCurrentToolObject()
    {
        currentToolObject.SetActive(false);
        currentToolObject = null;

        All_AllToolInActive();
    }

    private void All_AllToolInActive()
    {
        foreach (var tool in toolDict)
        {
            if (tool.Value != null)
                tool.Value.SetActive(false);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestSetNickname(string nickname) => this.nickname = nickname;

    void All_OnChangedNickname() => nicknameUI.SetNickname(nickname);

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

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestSetCurrentTool(int toolID = -1, string toolName = "", int toolAtt = 0)
    {
        if (toolID == -1)
        {
            CurrentToolID = -1;
            CurrentToolName = string.Empty;
            CurrentToolAtt = 0;
            return;
        }

        CurrentToolID = toolID;
        CurrentToolName = toolName;
        CurrentToolAtt = toolAtt;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_NotifyEquipmentTool(int itemIdx = -1)
    {
        All_AllToolInActive();

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

    /// <summary>
    /// UI 열림상태 설정
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestSetUIOpen(bool isOpen) => IsUIOpen = isOpen;

    /// <summary>
    /// 커서 잠금상태 설정
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestCursorLocked(bool isLocked) => IsCursorLocked = isLocked;

    /// <summary>
    /// 죽거나 잘 때 카메라 위치
    /// </summary>
    public void Client_SleepDeadCameraTarget(bool attachCamera, bool isSleep)
    {
        if (HasInputAuthority)
            cameraManager.SleepDeadCameraTarget(attachCamera, isSleep);
    }

    /// <summary>
    /// UI끄기
    /// </summary>
    public void Client_TurnOffInteractiveUI()
    {
        if (HasInputAuthority)
            playerInteractUI.Client_TurnOffInteractiveUI();
    }

    /// <summary>
    /// 활 있는지
    /// </summary>
    public void Host_SetHasArrow(NetworkBool hasArrow) => HasArrow = hasArrow;

    /// <summary>
    /// 던지는 돌맹이 있는지
    /// </summary>
    public void Host_SetHasPebble(NetworkBool hasPebble) => HasPebble = hasPebble;

    public void Host_NewDayStatus()
    {
        // 체력은 최대 체력의 80프로까지
        // huunger, thist는 일정 수치 감소
        Hp = playerData.maxHp;
        Hunger -= hungerSleepDecrease;
        Thirst -= thirstSleepDecrease;
    }

    /// <summary>
    /// 활 있는지
    /// </summary>
    public void RPC_RequestSetHasArrow(NetworkBool hasArrow) => HasArrow = hasArrow;

    /// <summary>
    /// 던지는 돌맹이 있는지
    /// </summary>
    public void RPC_RequestSetHasPebble(NetworkBool hasPebble) => HasPebble = hasPebble;

    /// <summary>
    /// 리스폰 장소 설정
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestSetRespawnPos(Vector3 respawnPos) { RespawnPos = respawnPos; }

    /// <summary>
    /// 스테이터스 회복
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestHeal()
    {
        Hp = playerData.maxHp;
        Stamina = playerData.maxStamina;
        Thirst = playerData.maxThirst;
        Hunger = playerData.maxHunger;
        Temperature = playerData.maxTemperature;
    }

    /// <summary>
    /// Bed.CanSleep 설정
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestCanSleep_Bed(Bed bed, NetworkBool canSleep) => bed.CanSleep = canSleep;

    /// <summary>
    /// 현재 침대 설정
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestCurrentBed(Bed bed) => Host_currentBed = bed;
}
