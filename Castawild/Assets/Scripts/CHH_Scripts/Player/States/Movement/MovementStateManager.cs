using Fusion;
using UnityEngine;

public enum MoveAnimationState { Idle, Walk, Run, CrouchIdle, CrouchWalk, IdleJump, RunJump, Sleep, Death, GetHit }

public class MovementStateManager : BaseStateManager
{
    #region Conponent
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
    #endregion

    #region Movement
    [Header("Movement")]
    public float currentMoveSpeed;
    public float airSpeedMuliplier = 0.7f;
    public float walkSpeed = 3f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;
    public float rotationSpeed = 10f;
    [HideInInspector] public bool canJump = true;
    #endregion

    [Space]
    [HideInInspector] public bool isJumping;

    #region GoundCheck
    [Header("GoundCheck")]
    [SerializeField] private float groundYOffset;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float fallMultiplier = 1.5f;
    private Vector3 spherePos;
    #endregion

    #region Animation
    [Header("Animation")]
    [SerializeField] private float animationLerpSpeed = 10f;
    [HideInInspector] public bool isLyingOrGettingUp; // 눕거나 일어나는 애니메이션 도중 카메라 못움직이게 확인하는 불변수
    #endregion

    #region Network
    [Header("Networked")]
    [Networked, HideInInspector] public bool Revived { get; set; }
    [Networked] public MoveAnimationState CurrentMoveState { get; set; }
    [Networked, HideInInspector] public bool JumpTriggered { get; set; }
    [Networked, HideInInspector] public bool CanWakeUp { get; set; }
    [Networked, HideInInspector] public Vector2 MoveValue { get; set; }
    #endregion

    public float Stamina
    {
        get => player.Stamina;
        set => player.Stamina = value;
    }

    protected override void Awake()
    {
        base.Awake();
        InitComponents();
        InitStates();
    }

    private void InitComponents()
    {
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
    }

    public override void Spawned()
    {
        ChangeState(idleState);
    }

    public void UpdateMoveAnimation(float deltaTime)
    {
        if (player.CanMoving())
        {
            anim.SetFloat("Horizontal", MoveValue.x, 0.1f, deltaTime);
            anim.SetFloat("Vertical", MoveValue.y, 0.1f, deltaTime);
        }

        anim.SetBool("Revived", Revived);
        anim.SetBool("Walking", false);
        anim.SetBool("Running", false);
        anim.SetBool("Crouching", false);
        anim.SetBool("Falling", false);
        anim.SetBool("Sleeping", false);

        switch (CurrentMoveState)
        {
            case MoveAnimationState.Walk:
                anim.SetBool("Walking", true);
                break;
            case MoveAnimationState.Run:
                anim.SetBool("Running", true);
                break;
            case MoveAnimationState.CrouchIdle:
                anim.SetBool("Crouching", true);
                break;
            case MoveAnimationState.CrouchWalk:
                anim.SetBool("Crouching", true);
                anim.SetBool("Walking", true);
                break;
            case MoveAnimationState.IdleJump:
                if (!IsTriggerSet)
                    anim.SetTrigger("IdleJump");
                IsTriggerSet = true;
                break;
            case MoveAnimationState.RunJump:
                if (!IsTriggerSet)
                    anim.SetTrigger("RunJump");
                IsTriggerSet = true;
                break;
            case MoveAnimationState.Sleep:
                anim.SetBool("Sleeping", true);
                break;
            case MoveAnimationState.GetHit:
                if (!IsTriggerSet)
                    anim.SetTrigger("GetHit");
                IsTriggerSet = true;
                break;
            case MoveAnimationState.Death:
                if (!IsTriggerSet)
                    anim.SetTrigger("Death");
                IsTriggerSet = true;
                break;
        }

        if (player.CanMoving())
        {
            // 이 부분 없으면 착지할 때 애니메이션 전환 이상함
            if (input.moveValue != Vector2.zero)
            {
                if (input.IsDown(PlayerNetworkInputData.sprintInput) && isJumping)
                    anim.SetBool("Running", true);
                else
                    anim.SetBool("Walking", true);
            }
        }
        anim.SetBool("Falling", !playerController.Grounded);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spherePos, groundCheckRadius);
    }

    /// <summary>
    /// 음식같은걸로 속도 바꿀 때 호출
    /// </summary>
    public void ChangeMoveSpeedValues(float value, bool isIncreasing)
    {
        if (isIncreasing)
        {
            walkSpeed += value;
            runSpeed += value;
            crouchSpeed += value;
        }
        else
        {
            walkSpeed -= value;
            runSpeed -= value;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ChangeSleepState(PlayerRef playerRef)
    {
        ChangeState(sleepState);
    }

    public bool HasEnoughStaminaToRun()
    {
        if (Stamina <= player.playerData.maxStamina * 0.3f)
            return false;

        return true;
    }

    public bool IsDeath() => CurrentMoveState == MoveAnimationState.Death;

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Revived()
    {
        ChangeState(idleState);
        player.Revived();
    }

    public bool CanRecoverStamina() => currentState != runState && currentState != deathState;
}
