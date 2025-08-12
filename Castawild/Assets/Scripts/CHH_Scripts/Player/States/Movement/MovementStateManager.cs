using Fusion;
using System.Collections.Generic;
using UnityEngine;

public enum MovementState { None, Idle, Walk, Run, Crouch, Jump, Sleep, Death, GetHit, Gather }
public enum MoveAnimatoinState { None, Idle, Walk, Run, CrouchIdle, CrouchWalk, IdleJump, RunJump, Sleep, Death, GetHit }

public class MovementStateManager : BaseStateManager
{
    #region Conponent
    private DayNightCycleManager dayNightManager;
    public PlayerInteractManager interactManager;
    #endregion

    #region States
    [Header("State")]
    public MovementState previousState;
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
    public MovementBaseState currentState;
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
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float fallMultiplier = 1.5f;
    private Vector3 spherePos;
    #endregion

    #region Network
    [Header("Networked")]
    [Networked, OnChangedRender(nameof(OnCurrentMoveStateChanged))]
    public MovementState CurrentMoveState { get; set; } = MovementState.None;
    [Networked, HideInInspector] public bool CanLanding { get; set; }
    [Networked] public MoveAnimatoinState CurrentMoveAnimation { get; set; }
    [Networked, HideInInspector] public bool Revived { get; set; }
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
    }

    public override void Spawned()
    {
        InitComponents();

        if (HasStateAuthority)
        {
            player.Host_TakeDamagedEvent -= ChangeToDeathState;
            player.Host_TakeDamagedEvent += ChangeToDeathState;

            DayNightCycleManager.OnTimeSkipStarted -= Host_WakeUp;
            DayNightCycleManager.OnTimeSkipStarted += Host_WakeUp;
        }

        Host_ChangeState(MovementState.Idle);
        OnCurrentMoveStateChanged();
    }

    private void ChangeToDeathState(bool isDeath)
    {
        if (isDeath)
            Host_ChangeState(MovementState.Death);
        else
            Host_ChangeState(MovementState.GetHit);
    }

    private void Host_WakeUp()
    {
        Host_ChangeState(MovementState.Idle);
        player.Host_NewDayStatus();
    }

    private void InitComponents()
    {
        if (HasStateAuthority)
            dayNightManager = FindAnyObjectByType<DayNightCycleManager>();
        interactManager = GetComponent<PlayerInteractManager>();
    }

    private void InitStates()
    {
        idleState = new IdleState(this);
        walkState = new WalkState(this);
        runState = new RunState(this);
        crouchState = new CrouchState(this);
        jumpState = new JumpState(this);
        sleepState = new SleepState(this);
        getHitState = new GetHitState(this);
        deathState = new DeathState(this);
        gatherState = new GatherState(this);

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
        if (!HasStateAuthority || CurrentMoveState == newState)
            return;
        CurrentMoveState = newState;
    }

    private void OnCurrentMoveStateChanged()
    {
        if (movementStateDict.TryGetValue(CurrentMoveState, out var newState))
        {
            currentState?.ExitState();
            currentState = newState;
            currentState.EnterState();
        }
    }

    /// <summary>
    /// 애니메이션 업데이트
    /// </summary>
    public void All_UpdateMoveAnimation(float deltaTime)
    {
        if (moveManager.All_CanMoving())
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

        if (moveManager.All_CanMoving())
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
        anim.SetBool("Falling", !moveManager.Grounded);
    }

    /// <summary>
    /// 달릴 수 있는지 체크
    /// </summary>
    public bool All_CanRun()
    {
        if (moveManager.CanRun_Tool && Stamina > player.playerData.maxStamina * 0.3f)
            return true;

        return false;
    }

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
