using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;



public enum ToolState { None, Idle, Aim, UseTool, Carry, Eat, Drink }
// 재생해야할 애니메이션 상태
public enum ToolAnimationState { None, Idle, Aim, FullAim, UseTool, Carry, Eat, Drink }

public class ToolStateManager : BaseStateManager
{
    #region Components
    [HideInInspector] public MovementStateManager movementManager;
    #endregion

    #region States
    public ToolIdleState idleState;
    public UseToolState useToolState;
    public AimState aimState;
    public CarryState carryState;
    public EatState eatState;
    public Dictionary<ToolState, ToolBaseState> toolStateDict;
    public ToolBaseState currentState;
    #endregion

    [Header("Player")]
    public Transform armature;
    [SerializeField] private GameObject Client_armMesh;

    [Header("Bow")]
    [SerializeField] private Animator bowAnim;

    [Header("Throw")]
    [SerializeField] private float throwUpForce = 0.3f;
    [SerializeField] private float arrowUpForce = 0.3f;
    [SerializeField] private float throwForce = 20f;
    [SerializeField] private float arrowForce = 30f;
    [SerializeField] private ThrowObject throwableStonePrefab;
    [SerializeField] private ThrowObject arrowPrefab;
    [SerializeField] private Transform firstPersonArrowPos;
    [SerializeField] private Transform thirdPersonArrowPos;
    [SerializeField] private Transform throwPos;

    [HideInInspector] public bool CanComboAttack { get; set; }
    [HideInInspector] public bool CanReceiveInput { get; set; }
    #region Network

    [Header("Network")]
    [Networked, OnChangedRender(nameof(OnCurrentToolStateChanged))]
    public ToolState CurrentToolState { get; set; }
    [Networked] public ToolAnimationState CurrentToolAnimationState { get; set; }
    [Networked, HideInInspector] public bool ComboAttack { get; set; }
    [Networked, HideInInspector] public bool DecreaseToolDuration { get; set; }
    #endregion

    public ToolType CurrentToolType
    {
        get => toolManager.CurrentToolType;
        set => toolManager.CurrentToolType = value;
    }

    protected override void Awake()
    {
        base.Awake();

        InitComponents();
        InitStates();
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            player.Host_TakeDamagedEvent -= ChangeToDeathState;
            player.Host_TakeDamagedEvent += ChangeToDeathState;

        }

        toolManager.Host_ChangeSelectedItem += All_SetCurrentHoldItem;

        CurrentToolType = ToolType.Fist;
        Host_ChangeState(ToolState.Idle);
        OnCurrentToolStateChanged();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        toolManager.Host_ChangeSelectedItem -= All_SetCurrentHoldItem;
    }

    private void ChangeToDeathState(bool isDeath) => Host_ChangeState(ToolState.Idle);

    private void InitComponents() => movementManager = GetComponent<MovementStateManager>();

    private void InitStates()
    {
        idleState = new ToolIdleState(this);
        useToolState = new UseToolState(this);
        aimState = new AimState(this);
        carryState = new CarryState(this);
        eatState = new EatState(this);

        toolStateDict = new Dictionary<ToolState, ToolBaseState>
        {
            { ToolState.Idle, idleState },
            { ToolState.Aim, aimState },
            { ToolState.UseTool, useToolState },
            { ToolState.Carry, carryState },
            { ToolState.Eat, eatState }
        };
    }

    /// <summary>
    /// 상태 변환
    /// </summary>
    public void Host_ChangeState(ToolState newState)
    {
        if (!HasStateAuthority || newState == CurrentToolState)
            return;

        CurrentToolState = newState;
    }

    private void OnCurrentToolStateChanged()
    {
        if (toolStateDict.TryGetValue(CurrentToolState, out var newState))
        {
            currentState?.ExitState();
            currentState = newState;
            currentState.EnterState();
        }
    }

    /// <summary>
    /// 애니메이션 업데이트
    /// </summary>
    public void All_UpdateMoveAnimation()
    {
        anim.SetBool("Aiming", false);
        anim.SetBool("FullAiming", false);
        anim.SetBool("FullUseTool", false);
        anim.SetBool("Carrying", false);

        switch (CurrentToolAnimationState)
        {
            case ToolAnimationState.Aim:
                anim.SetInteger("WeaponType", (int)toolManager.CurrentToolType);
                anim.SetBool("Aiming", true);
                break;
            case ToolAnimationState.FullAim:
                anim.SetInteger("WeaponType", (int)toolManager.CurrentToolType);
                anim.SetBool("FullAiming", true);
                break;
            case ToolAnimationState.UseTool:
                if (input.IsDown(PlayerNetworkInputData.aimInput) && toolManager.All_HoldAimTool())
                {
                    anim.SetInteger("WeaponType", (int)toolManager.CurrentToolType);
                    anim.SetBool("Aiming", true);
                    anim.SetBool("FullAiming", true);
                }
                anim.SetInteger("WeaponType", (int)toolManager.CurrentToolType);
                anim.SetBool("FullUseTool", true);
                break;
            case ToolAnimationState.Carry:
                anim.SetBool("Carrying", true);
                break;
            case ToolAnimationState.Eat:
                anim.SetTrigger("Eating");
                CurrentToolAnimationState = ToolAnimationState.None;
                break;
            case ToolAnimationState.Drink:
                anim.SetTrigger("Drinking");
                CurrentToolAnimationState = ToolAnimationState.None;
                break;
        }

        anim.SetBool("ComboAttack", ComboAttack);
    }

    /// <summary>
    /// 팔 Mesh 활성화/비활성화
    /// </summary>
    public void Client_ArmVisibleChanged(bool isVisible)
    {
        if (cameraManager.currentView != ViewType.FirstPerson || !HasInputAuthority)
            return;

        Client_armMesh.SetActive(isVisible);
    }
    /// <summary>
    /// 돌맹이/화살 생성
    /// </summary>
    public void Host_SpawnThrowObject(bool isArrow, Vector3 rayTargetPos)
    {
        if (!HasStateAuthority)
            return;

        NetworkObject throwObject;
        if (isArrow && toolManager.HasArrow)
        {
            if (input.currentView == ViewType.FirstPerson)
            {
                throwObject = Runner.Spawn(arrowPrefab.gameObject, firstPersonArrowPos.position, cameraManager.firstPersonCam.transform.rotation);
                throwObject?.GetComponent<ThrowObject>().AddForce(arrowForce, arrowUpForce, rayTargetPos);
            }
            else
            {
                throwObject = Runner.Spawn(arrowPrefab.gameObject, thirdPersonArrowPos.position, cameraManager.thirdPersonCam.transform.rotation);
                throwObject?.GetComponent<ThrowObject>().AddForce(arrowForce, arrowUpForce, rayTargetPos);
            }

            player.inventory.UseItem(201, 1);
            if (player.inventory.GetItemCount(201) <= 0)
            {
                toolManager.HasArrow = false;
                toolManager.RPC_NotifyArrowActive(false);
            }
        }
        else if (!isArrow)
        {
            if (input.currentView == ViewType.FirstPerson)
                throwObject = Runner.Spawn(throwableStonePrefab.gameObject, throwPos.position, cameraManager.firstPersonCam.transform.rotation);
            else
                throwObject = Runner.Spawn(throwableStonePrefab.gameObject, throwPos.position, cameraManager.thirdPersonCam.transform.rotation);

            throwObject?.GetComponent<ThrowObject>().AddForce(throwForce, throwUpForce, rayTargetPos);
            throwObject.GetComponent<ThrowObject>().thrower = player.gameObject;

            player.inventory.UseItem(202, 1);

            if (player.inventory.GetItemCount(202) <= 0)
            {
                Debug.Log("돌맹이 없음, 주먹으로 변경");
                toolManager.Host_InitCurrentTool();
                CurrentToolType = ToolType.Fist;
            }
        }
    }

    /// <summary>
    /// Ray로 조준 위치 설정
    /// </summary>
    public void Client_Throw(int isArrow)
    {
        if (isArrow == 1)
            RPC_NotifySetArrowPull(false);

        if (!HasInputAuthority)
            return;

        SoundManager.Instance.PlayLocalSound3D(Object.InputAuthority, Sound.Player_Shoot, transform.position);

        if (cameraManager.currentView == ViewType.FirstPerson && toolManager.HasArrow)
            cameraManager.ShakeCamera(transform.right, 0.1f);

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        Vector3 rayTargetPos = ray.GetPoint(30f);

        RPC_RequestThrow(isArrow, rayTargetPos);
    }

    /// <summary>
    /// Aim 카메라,UI 설정
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ApplySetAimCameraAndUI(bool aimStart)
    {
        cameraManager.MoveAimCamera(aimStart);
        interactUI.SetAimCrosshair(aimStart);
    }

    public void Client_SetAimCameraAndUI(bool aimStart)
    {
        cameraManager.MoveAimCamera(aimStart);
        interactUI.SetAimCrosshair(aimStart);
    }

    /// <summary>
    /// 활 조준 설정
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotifySetArrowPull(bool isAiming)
    {
        bowAnim.SetBool("Pull", isAiming);
        toolManager.All_SetArrowActive(isAiming);
    }

    /// <summary>
    /// 활 쏘는 애니메이션 트리거
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotifyBowShootAnimation() => bowAnim.SetTrigger("Shoot");

    public void Host_RotatePlayer(bool rotate) => moveManager.RotatePlayer();


    /// <summary>
    /// 던지기
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestThrow(int isArrow, Vector3 rayTargetPos)
    {
        Host_SpawnThrowObject(isArrow == 0 ? false : true, rayTargetPos);

        if (isArrow == 0)
            toolManager.All_SetPebbleActive(false);
        else
            toolManager.arrow.SetActive(false);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestDecreaseToolDuration(bool isDecreased)
       => DecreaseToolDuration = isDecreased;


    /// <summary>
    /// 현재 아이템 변경 
    /// </summary>
    public void All_SetCurrentHoldItem(int itemIdx = -1)
    {
        // 설치가능한 아이템 
        if (itemIdx >= 300 && itemIdx < 400)
        {
            if (HasStateAuthority)
                Host_ChangeState(ToolState.Carry);
            return;
        }

        switch (itemIdx)
        {
            case 202: // 짱돌 
                CurrentToolType = ToolType.Throw;
                break;
            case 401: // 방망이
                CurrentToolType = ToolType.Sword;
                break;
            case 402: // 횃불
                CurrentToolType = ToolType.Sword;
                break;
            case 403: // 돌도끼
                CurrentToolType = ToolType.Axe;
                break;
            case 404: // 돌작살
                CurrentToolType = ToolType.Spear;
                break;
            case 405: // 돌곡괭이
                CurrentToolType = ToolType.Pickaxe;
                break;
            case 406: // 활
                {
                    CurrentToolType = ToolType.Bow;
                    toolManager.arrow.SetActive(false);
                }
                break;
            default:
                CurrentToolType = ToolType.Fist;
                break;
        }
    }
}
