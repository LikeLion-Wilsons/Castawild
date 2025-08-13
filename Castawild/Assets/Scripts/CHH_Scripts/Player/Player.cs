using Fusion;
using System;
using System.Collections;
using Test;
using UnityEngine;

// 테스트용
public enum MoveType { Idle, Walk, Run, Crouch, Jump }
public enum AttackType { None, Aim, Attack }
public enum ItemType { None, Default, Tool, Food, Drink, Placeable }
public enum StatType { Hp, Stamina, Hunger, Thirst }

[DisallowMultipleComponent]
public class Player : NetworkBehaviour
{
    #region Components
    [HideInInspector] public Animator anim;
    [HideInInspector] public DayNightCycleManager dayNightManager;
    [HideInInspector] public PlayerInteractUI playerInteractUI;
    [HideInInspector] public PlayerCameraManager cameraManager;
    [HideInInspector] public PlayerFlagManager flagManager;
    #endregion

    #region Status
    [Header("NickName")]
    [SerializeField] private NicknameUI nicknameUI;
    [Networked, OnChangedRender(nameof(All_OnChangedNickname))] private string nickname { get; set; }

    [Header("Status")]
    public PlayerData playerData = new PlayerData();

    [Header("Current Status")]
    [Networked] public float Hp { get; set; }
    [Networked] public float Stamina { get; set; }
    [Networked] public float Hunger { get; set; }
    [Networked] public float Thirst { get; set; }
    [Networked] public float Temperature { get; set; }
    #endregion

    [Space]
    [SerializeField] private float foodRestoreTime = 5f;
    public ItemType currentItemType; // 이걸로 도구 장착 
    [Networked] public FoodInfoData currentFoodInfoData { get; set; } // 지금 들고있는 음식 정보 -> 인벤 아이템 인덱스로 가져옴

    [Header("Recovery Rate")]
    public float staminaRecoveryRate = 2f;
    public float temperatureReceoveryRate = 1f;

    [Header("Decrease Rate")]
    public float hpDecreaseRate = 0.5f;
    public float staminaHungerDecreaseRate = 3f;
    public float staminaRunDecreaseRate = 1f;
    public float hungerDecreaseRate = 1f;
    public float thirstDecreaseRate = 1f;
    public float temperatureDecreaseRate = 1f;

    [Header("Sleep Rate")]
    public float thirstSleepDecrease = 10f;
    public float hungerSleepDecrease = 10f;

    #region Interact
    [Header("Interact")]
    [HideInInspector] public Bed Host_currentBed;
    #endregion

    public ScreenEffect screenEffect;
    public GameObject amarture;

    public PlayerNetworkInputData input { get; set; }
    [Header("Networked")]
    [Networked, HideInInspector] public Vector3 RespawnPos { get; set; }
    [Networked, HideInInspector] public bool CanPVP { get; set; } = true;
    [Networked, HideInInspector] public bool IsUIOpen { get; set; }
    [Networked, HideInInspector] public bool IsCursorLocked { get; set; }

    [HideInInspector] public InventoryDataManager inventory;
    [HideInInspector] public bool isSpawned;

    [HideInInspector] public bool isNearFire;
    [HideInInspector] public UIStats uiStats;

    public event Action<bool> Host_TakeDamagedEvent;
    public event Action Host_DecreaseToolDuration;
    public event Action ClearCup;

    public Coroutine fallingCoroutine;
    private Coroutine foodRestoreCoroutine;

    // Test
    [Networked] private bool IsCooling { get; set; } = false;

    private void Update()
    {
        if (!HasInputAuthority)
            return;
        // 테스트용
        if (Input.GetKeyDown(KeyCode.K))
            Host_TakeDamage(true, 10);

        if (Input.GetKeyDown(KeyCode.H))
        {
            screenEffect.TakeDamageEffect(0f);
            RPC_RequestHeal();
        }

        if (Input.GetKeyDown(KeyCode.O))
            RPC_RequestCool();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestCool()
    {
        IsCooling = !IsCooling;
        Debug.Log("IsCooling : " + IsCooling);
    }

    override public void Spawned()
    {
        InitComponents();

        isSpawned = true;
        InitStatus();

        RespawnPos = transform.position;

        if (HasInputAuthority)
            RPC_RequestSetNickname(PlayerTempData.nickname);

        All_OnChangedNickname();

        if (HasInputAuthority)
            nicknameUI.gameObject.SetActive(false);
    }

    public void Init() { }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestSetNickname(string nickname) => this.nickname = nickname;

    public override void FixedUpdateNetwork()
    {
        if (flagManager.IsDead)
            return;

        if (HasStateAuthority)
            UpdateStats();

        if (HasInputAuthority)
        {
            screenEffect.ContinuousDamageEffect(Hp, playerData.maxHp);
            screenEffect.ContinuousColdEffect(Temperature, playerData.maxTemperature);
        }
    }

    private void UpdateStats()
    {
        if (Thirst <= 0)
            Stamina -= staminaHungerDecreaseRate * Runner.DeltaTime;
        else
            Thirst -= thirstDecreaseRate * Runner.DeltaTime;

        if (Hunger <= 0 || Temperature <= 0)
        {
            Host_TakeDamage(false, hpDecreaseRate * Runner.DeltaTime);
            Stamina -= staminaHungerDecreaseRate * Runner.DeltaTime;
        }
        else if (Hunger > 0)
            Hunger -= hungerDecreaseRate * Runner.DeltaTime;

        if (flagManager.CanRecoverStamina)
        {
            if (Stamina < playerData.maxStamina)
                Stamina += staminaRecoveryRate * Runner.DeltaTime;
            else
                Stamina = playerData.maxStamina;
        }

        if (dayNightManager == null)
            return;

        if ((IsCooling || (dayNightManager.isNightTime && !isNearFire)) && Temperature > 0f)
            Temperature -= temperatureDecreaseRate * Runner.DeltaTime;

        else if ((!IsCooling || !dayNightManager.isNightTime || isNearFire) && Temperature < playerData.maxTemperature)
            Temperature += temperatureReceoveryRate * Runner.DeltaTime;
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

            attackObject.player.Host_DecreaseToolDuration?.Invoke();
            attackObject.player.GetComponent<PlayerInteractManager>().RPC_ApplyHitInvoke(attackObject.Att);
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
                Host_TakeDamage(true, throwObject.Att);
                throwObject.canAttack = false;
                throwObject.player.GetComponent<PlayerInteractManager>().RPC_ApplyHitInvoke(throwObject.Att);
            }
        }
    }

    private void InitComponents()
    {
        if (HasStateAuthority)
            dayNightManager = FindAnyObjectByType<DayNightCycleManager>();
        anim = GetComponentInChildren<Animator>();
        playerInteractUI = GetComponentInChildren<PlayerInteractUI>();
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
        inventory = GetComponent<InventoryDataManager>();
        flagManager = GetComponent<PlayerFlagManager>();
        playerToolManager = GetComponent<PlayerToolManager>();
        toolStateManager = new ToolStateManager();
    }

    private void InitStatus()
    {
        Hp = playerData.maxHp;
        Stamina = playerData.maxStamina;
        Hunger = playerData.maxHunger;
        Thirst = playerData.maxThirst;
        Temperature = playerData.maxTemperature;
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
    /// 커서 잠구기
    /// </summary>
    public void Client_SetCursorLocked(bool isLocked)
    {
        if (HasInputAuthority)
            RPC_RequestCursorLocked(isLocked);
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
    /// 리스폰 위치 설정
    /// </summary>
    public void All_SetRespawnPos(Vector3 respawnPos)
    {
        if (HasInputAuthority)
            RPC_RequestSetRespawnPos(respawnPos);
        else
            RespawnPos = respawnPos;
    }

    /// <summary>
    /// 부활 스테이터스
    /// </summary>
    public void Host_RevivedStatus()
    {
        flagManager.Clear(PlayerFlags.Death);
        Hp = playerData.maxHp * 0.2f;
        Stamina = playerData.maxStamina;
        Thirst = playerData.maxThirst * 0.2f;
        Hunger = playerData.maxHunger * 0.2f;

        screenEffect.TakeDamageEffect(0f);
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
            flagManager.Set(PlayerFlags.Death);
            Host_TakeDamagedEvent?.Invoke(true);
            return;
        }
        else if (Hp > 0 && isAttack)
        {
            Host_TakeDamagedEvent?.Invoke(false);

            if (isAttack)
            {
                RPC_ApplyPlayDamagedEffectAnim();
                RPC_NotifyPlayDamagedAnim();

                if (input.currentView == ViewType.FirstPerson)
                    RPC_ApplyShakeCamera(transform.right, 0.5f);
                else
                    RPC_ApplyShakeCamera(transform.right, 0.3f);
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_ApplyPlayDamagedEffectAnim() => screenEffect.SetTrigger("Damaged");

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotifyPlayDamagedAnim() => anim.SetTrigger("GetHit");

    public void Host_RestoreStatFromFood()
    {
        if (foodRestoreCoroutine != null)
            StopCoroutine(foodRestoreCoroutine);

        foodRestoreCoroutine = StartCoroutine(RestoreStatFromFoodCoroutine());

        inventory.RPC_ClearCup();
        ClearCup?.Invoke();
    }

    private IEnumerator RestoreStatFromFoodCoroutine()
    {
        float elapsed = 0f;
        float restoreHPPerSecond = currentFoodInfoData.restoreHPValue / foodRestoreTime;
        float restoreStaminaPerSecond = currentFoodInfoData.restoreStaminaValue / foodRestoreTime;
        float restoreHungerPerSecond = currentFoodInfoData.restoreHungerValue / foodRestoreTime;
        float restoreThirstPerSecond = currentFoodInfoData.restoreThirstValue / foodRestoreTime;

        if (uiStats != null)
        {
            uiStats.SetBarColor(StatType.Hp, currentFoodInfoData.restoreHPValue);
            uiStats.SetBarColor(StatType.Stamina, currentFoodInfoData.restoreStaminaValue);
            uiStats.SetBarColor(StatType.Hunger, currentFoodInfoData.restoreHungerValue);
            uiStats.SetBarColor(StatType.Thirst, currentFoodInfoData.restoreThirstValue);
        }

        while (elapsed < foodRestoreTime)
        {
            elapsed += Runner.DeltaTime;

            if (restoreHPPerSecond != 0)
                Hp = RestoreValue(restoreHPPerSecond, Hp, playerData.maxHp);
            if (restoreStaminaPerSecond != 0)
                Stamina = RestoreValue(restoreStaminaPerSecond, Stamina, playerData.maxStamina);
            if (restoreHungerPerSecond != 0)
                Hunger = RestoreValue(restoreHungerPerSecond, Hunger, playerData.maxHunger);
            if (restoreThirstPerSecond != 0)
                Thirst = RestoreValue(restoreThirstPerSecond, Thirst, playerData.maxThirst);

            yield return null;
        }

        if (uiStats != null)
        {
            uiStats.SetBarColor(StatType.Hp, 0);
            uiStats.SetBarColor(StatType.Stamina, 0);
            uiStats.SetBarColor(StatType.Hunger, 0);
            uiStats.SetBarColor(StatType.Thirst, 0);
        }

        foodRestoreCoroutine = null;
    }

    private float RestoreValue(float restoreHPPerSecond, float currentValue, float maxValue)
    {
        float restoreValue = restoreHPPerSecond * Runner.DeltaTime;
        return Mathf.Min(currentValue + restoreValue, maxValue);
    }

    private void All_OnChangedNickname() => nicknameUI.SetNickname(nickname);

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
    /// 다음날 스테이터스
    /// </summary>
    public void Host_NewDayStatus()
    {
        // 체력은 최대 체력의 80프로까지
        // huunger, thist는 일정 수치 감소
        Hp = playerData.maxHp;
        Hunger -= hungerSleepDecrease;
        Thirst -= thirstSleepDecrease;
    }

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

        screenEffect.TakeDamageEffect(0f);
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

    /// <summary>
    /// 카메라 쉐이크
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ApplyShakeCamera(Vector3 direction, float force) => cameraManager.ShakeCamera(direction, force);



    // 임시 
    [HideInInspector] public ToolStateManager toolStateManager;

    PlayerToolManager playerToolManager;
    public void Client_RemoveSelectedItem() => playerToolManager.Client_RemoveSelectedItem();
    public void Client_ApplySelectedItem(int value) => playerToolManager.Client_ApplySelectedItem(value);
    public void Host_SetHasArrow(bool value) => playerToolManager.Host_SetHasArrow(value);
    public void Host_SetHasPebble(bool value) => playerToolManager.Host_SetHasPebble(value);
    public void RPC_RequestSetHasArrow(bool value) => playerToolManager.RPC_RequestSetHasArrow(value);
    public void RPC_RequestSetHasPebble(bool value) => playerToolManager.RPC_RequestSetHasPebble(value);
}

// 임시
public class PlayerController
{

}