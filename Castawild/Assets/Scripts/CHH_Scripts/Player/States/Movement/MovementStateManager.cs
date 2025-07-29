using Fusion;
using UnityEngine;

public enum MoveAnimationState { Idle, Walk, Run, CrouchIdle, CrouchWalk, IdleJump, RunJump }

public class MovementStateManager : BaseStateManager
{
    #region Conponent
    [HideInInspector] public ToolStateManager toolStateManager;
    [HideInInspector] public KCCPlayerController playerController;
    #endregion

    #region States
    public MovementBaseState previousState;
    public IdleState idleState;
    public WalkState walkState;
    public RunState runState;
    public JumpState jumpState;
    public CrouchState crouchState;
    #endregion

    #region Movement
    public float currentMoveSpeed;
    public float airSpeedMuliplier = 0.7f;
    public float walkSpeed = 3f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;
    public float rotationSpeed = 10f;
    [HideInInspector] public bool canJump = true;

    public float sensitivity = 1.5f;
    public float maxXRotation = 80f;
    public float minXRotation = -80f;
    #endregion

    #region GoundCheck
    [SerializeField] private float groundYOffset;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float fallMultiplier = 1.5f;
    private Vector3 spherePos;
    #endregion

    #region Gravity
    public float gravity = -20f;
    public float jumpForce = 10f;
    [HideInInspector] public bool isJumping;
    [HideInInspector] public Vector3 velocity;
    #endregion

    #region Animation
    [SerializeField] private float animationLerpSpeed = 10f;
    public bool isTriggerSet = false;
    #endregion

    #region Network
    [Networked] public MoveAnimationState CurrentMoveState { get; set; }
    [Networked] public bool JumpTriggered { get; set; }
    [Networked] public bool CanMove { get; set; }
    [Networked] public Vector2 MoveValue { get; set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        InitComponents();
        InitStates();
    }

    private void InitComponents()
    {
        toolStateManager = GetComponent<ToolStateManager>();
        playerController = GetComponent<KCCPlayerController>();
    }

    private void InitStates()
    {
        idleState = new IdleState(this, inputManager);
        walkState = new WalkState(this, inputManager);
        runState = new RunState(this, inputManager);
        crouchState = new CrouchState(this, inputManager);
        jumpState = new JumpState(this, inputManager);
    }
    public override void Spawned()
    {
        ChangeState(idleState);
    }

    public void UpdateMoveAnimation(float deltaTime)
    {
        anim.SetFloat("Horizontal", MoveValue.x, 0.1f, deltaTime);
        anim.SetFloat("Vertical", MoveValue.y, 0.1f, deltaTime);

        anim.SetBool("Walking", false);
        anim.SetBool("Running", false);
        anim.SetBool("Crouching", false);
        anim.SetBool("Falling", false);

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
                if (!isTriggerSet)
                    anim.SetTrigger("IdleJump");
                isTriggerSet = true;
                break;
            case MoveAnimationState.RunJump:
                if (!isTriggerSet)
                    anim.SetTrigger("RunJump");
                isTriggerSet = true;
                break;
        }
        if (input.moveValue != Vector2.zero)
        {
            if (input.IsDown(PlayerNetworkInputData.sprintInput) && isJumping)
                anim.SetBool("Running", true);
            else
                anim.SetBool("Walking", true);
        }
        anim.SetBool("Falling", !playerController.Grounded);
    }

    public void RotatePlayer(Vector3 moveDir)
    {
        if (cameraManager.currentView == ViewType.FirstPerson && inputManager.isCursorLocked)
            transform.Rotate(Vector3.up * inputManager.lookInput.x * cameraManager.sensitivity);

        else if (cameraManager.currentView == ViewType.ThirdPerson &&
            moveDir.sqrMagnitude > 0.001f && !player.isAimLocked && playerController.Grounded)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 중력 적용
    /// </summary>
    public Vector3 Gravity()
    {
        if (playerController.Grounded && velocity.y < 0)
            velocity.y = -1f;
        else
            velocity.y += gravity * fallMultiplier * Time.fixedDeltaTime;

        return velocity;
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

}
