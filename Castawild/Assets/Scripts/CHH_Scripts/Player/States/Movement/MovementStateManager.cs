using Fusion;
using System.Collections.Generic;
using UnityEngine;

public enum MovementState { None, Idle, Walk, Run, Crouch, Jump, Sleep, Death, GetHit, Gather }
public enum MoveAnimatoinState { None, Idle, Walk, Run, CrouchIdle, CrouchWalk, IdleJump, RunJump, Sleep, Death, GetHit }

public class MovementStateManager : BaseStateManager
{
    #region Conponent
    private DayNightCycleManager dayNightManager;
    [HideInInspector] public PlayerInteractUI interactUI;
    [HideInInspector] public ToolStateManager toolStateManager;
    #endregion

    #region States
    [Header("State")]
    public MovementBaseState previousState;
    public IdleState idleState;
    public WalkState walkState;
    public RunState runState;
    public JumpState jumpState;
    public CrouchState crouchState;
    public SleepState sleepState;
    public GetHitState getHitState;
    public DeathState deathState;
    public GatherState gatherState;
    public Dictionary<MovementState, MovementBaseState> movementStateDict;
    public MovementBaseState currentState; // 호스트용 변수
    #endregion

    #region Movement
    [Header("Movement")]
    public float airSpeedMuliplier = 0.7f;
    public float walkSpeed = 3f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;
    public float rotationSpeed = 10f;
    #endregion

    #region GoundCheck
    [Header("GoundCheck")]
    [SerializeField] private float groundYOffset;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float fallMultiplier = 1.5f;
    private Vector3 spherePos;
    #endregion

    #region Network
    [Header("Networked")]
    [Networked, OnChangedRender(nameof(OnCurrentMoveStateChanged))]
    public MovementState CurrentMoveState { get; set; }
    [Networked] public bool CanLanding { get; set; }
    [Networked] public MoveAnimatoinState CurrentMoveAnimation { get; set; }
    [Networked] public float currentMoveSpeed { get; set; }
    [Networked, HideInInspector] public bool Revived { get; set; }
    [Networked, HideInInspector] public bool JumpTriggered { get; set; }
    [Networked, HideInInspector] public Vector2 MoveValue { get; set; }
    [Networked, HideInInspector] public bool kneel { get; set; }
    #endregion

    public float Stamina
    {
        get => player.Stamina;
        set => player.Stamina = value;
    }

    protected override void Awake()
    {
        base.Awake();
        InitStates();
        Host_ChangeState(MovementState.Idle);
    }

    public override void Spawned()
    {
        InitComponents();
        if (HasStateAuthority)
        {
            DayNightCycleManager.OnTimeSkipStarted -= Host_WakeUp;
            DayNightCycleManager.OnTimeSkipStarted += Host_WakeUp;
        }
    }

    private void InitComponents()
    {
        if (HasStateAuthority)
            dayNightManager = FindAnyObjectByType<DayNightCycleManager>();
        interactUI = GetComponentInChildren<PlayerInteractUI>();
        toolStateManager = GetComponent<ToolStateManager>();
    }

    private void InitStates()
    {
        idleState = new IdleState(this, inputManager);
        walkState = new WalkState(this, inputManager);
        runState = new RunState(this, inputManager);
        crouchState = new CrouchState(this, inputManager);
        jumpState = new JumpState(this, inputManager);
        sleepState = new SleepState(this, inputManager);
        getHitState = new GetHitState(this, inputManager);
        deathState = new DeathState(this, inputManager);
        gatherState = new GatherState(this, inputManager);

        movementStateDict = new Dictionary<MovementState, MovementBaseState>
        {
            { MovementState.Idle, idleState },
            { MovementState.Walk, walkState },
            { MovementState.Run, runState },
            { MovementState.Crouch, crouchState },
            { MovementState.Jump, jumpState },
            { MovementState.Sleep, sleepState },
            { MovementState.GetHit, getHitState },
            { MovementState.Death, deathState },
            { MovementState.Gather, gatherState }
        };
    }

    /// <summary>
    /// 상태 변경
    /// </summary>
    public void Host_ChangeState(MovementState newState)
    {
        if (!HasStateAuthority)
            return;
        CurrentMoveState = newState;
    }

    private void OnCurrentMoveStateChanged()
    {
        if (movementStateDict.TryGetValue(CurrentMoveState, out var newState))
        {
            if (currentState == newState)
                return;

            currentState?.ExitState();
            currentState = newState;
            currentState.EnterState();
        }
    }

    private void Host_WakeUp()
    {

        Host_ChangeState(MovementState.Idle);
        player.Host_NewDayStatus();
    }

    /// <summary>
    /// 애니메이션 업데이트
    /// </summary>
    public void All_UpdateMoveAnimation(float deltaTime)
    {
        if (player.All_CanMoving())
        {
            anim.SetFloat("Horizontal", MoveValue.x, 0.1f, deltaTime);
            anim.SetFloat("Vertical", MoveValue.y, 0.1f, deltaTime);
        }

        anim.SetBool("Revived", Revived);
        anim.SetBool("Walking", false);
        anim.SetBool("Running", false);
        anim.SetBool("Crouching", false);
        anim.SetBool("Falling", false);

        switch (CurrentMoveAnimation)
        {
            case MoveAnimatoinState.Walk:
                anim.SetBool("Walking", true);
                break;
            case MoveAnimatoinState.Run:
                anim.SetBool("Running", true);
                break;
            case MoveAnimatoinState.CrouchIdle:
                anim.SetBool("Crouching", true);
                break;
            case MoveAnimatoinState.CrouchWalk:
                anim.SetBool("Crouching", true);
                anim.SetBool("Walking", true);
                break;
            case MoveAnimatoinState.IdleJump:
                anim.SetTrigger("IdleJump");
                CurrentMoveAnimation = MoveAnimatoinState.None;
                break;
            case MoveAnimatoinState.RunJump:
                anim.SetTrigger("RunJump");
                CurrentMoveAnimation = MoveAnimatoinState.None;
                break;
            case MoveAnimatoinState.Death:
                anim.SetTrigger("Death");
                CurrentMoveAnimation = MoveAnimatoinState.None;
                break;
            case MoveAnimatoinState.Sleep:
                anim.SetTrigger("Sleep");
                CurrentMoveAnimation = MoveAnimatoinState.None;
                break;
        }

        if (player.All_CanMoving())
        {
            // 이 부분 없으면 착지할 때 애니메이션 전환 이상함
            if (input.moveValue != Vector2.zero)
            {
                if (input.IsDown(PlayerNetworkInputData.sprintInput) && All_CanRun())
                    anim.SetBool("Running", true);
                else
                    anim.SetBool("Walking", true);
            }
        }
        anim.SetBool("Falling", !playerController.Grounded);
    }

    public bool All_CanRun()
    {
        if (toolStateManager.CurrentToolState == ToolState.Aim || toolStateManager.CurrentToolState == ToolState.Carry
            || !All_HasEnoughStaminaToRun())
            return false;
        return true;
    }

    /// <summary>
    /// 달릴 수 있는 기력 체크
    /// </summary>
    public bool All_HasEnoughStaminaToRun()
    {
        if (Stamina <= player.playerData.maxStamina * 0.3f)
            return false;

        return true;
    }

    /// <summary>
    /// 스테미나 회복가능한지 확인
    /// </summary>
    public bool All_CanRecoverStamina() => CurrentMoveState != MovementState.Run && CurrentMoveState != MovementState.Death;

    public void Host_Sleep(bool isSleep) => dayNightManager.Rpc_SetSleepingState(isSleep, Object.InputAuthority);

    /// <summary>
    /// Sleep 상태로 변경하는 RPC
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestChangeSleepState(PlayerRef playerRef)
    {
        Host_ChangeState(MovementState.Sleep);
    }

    /// <summary>
    /// Gather상태로 변경하는 RPC
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestChangeGatherState(PlayerRef playerRef)
    {
        Host_ChangeState(MovementState.Gather);
    }

    /// <summary>
    /// 부활하는 RPC
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestRevived()
    {
        Host_ChangeState(MovementState.Idle);
        player.Host_RevivedStatus();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestSetKneel(NetworkBool _kneel) => kneel = _kneel;
}
