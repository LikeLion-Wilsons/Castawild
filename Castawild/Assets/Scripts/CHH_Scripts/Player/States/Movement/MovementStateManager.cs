using Fusion;
using UnityEngine;

public class MovementStateManager : BaseStateManager
{
    #region Conponent
    [HideInInspector] public ToolStateManager toolStateManager;
    [HideInInspector] public NetworkCharacterControllerCustom networkCharacterController;
    #endregion

    #region States
    public MovementBaseState previousState;
    public IdleState idleState;
    public WalkState walkState;
    public RunState runState;
    public JumpState jumpState;
    public CrouchState crouchState;
    public BaseState currentState;
    #endregion

    #region Movement
    [HideInInspector] public bool canMove = true;
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
    [HideInInspector] public bool jumped;
    [HideInInspector] public Vector3 velocity;
    #endregion

    #region Animation
    [SerializeField] private float animationLerpSpeed = 10f;
    public bool isTriggerSet = false;
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
        networkCharacterController = GetComponent<NetworkCharacterControllerCustom>();
    }

    private void InitStates()
    {
        idleState = new IdleState(this, inputManager);
        walkState = new WalkState(this, inputManager);
        runState = new RunState(this, inputManager);
        crouchState = new CrouchState(this, inputManager);
        jumpState = new JumpState(this, inputManager);

        ChangeState(idleState);
    }

    public void UpdateMoveAnimation(float deltaTime)
    {
        anim.SetFloat("Horizontal", networkManager.MoveValue.x, 0.1f, deltaTime);
        anim.SetFloat("Vertical", networkManager.MoveValue.y, 0.1f, deltaTime);

        anim.SetBool("Walking", false);
        anim.SetBool("Running", false);
        anim.SetBool("Crouching", false);
        anim.SetBool("Falling", false);

        switch (networkManager.CurrentMoveState)
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
        anim.SetBool("Falling", !networkCharacterController.Grounded);
    }

    /// <summary>
    /// 움직이는 방향
    /// </summary>
    public Vector3 GetMoveDir(Vector2 moveInput, bool isLocalPlayer)
    {
        if (!canMove)
            return Vector3.zero;

        Vector3 forward;
        Vector3 right;

        if (isLocalPlayer && cameraManager.currentView == ViewType.FirstPerson)
        {
            forward = transform.forward;
            right = transform.right;
        }
        else
        {
            forward = cameraManager.CurrenCam.transform.forward;
            right = cameraManager.CurrenCam.transform.right;
        }

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return forward * moveInput.y + right * moveInput.x;
    }

    public void RotatePlayer(Vector3 moveDir)
    {
        if (cameraManager.currentView == ViewType.FirstPerson && inputManager.isCursorLocked)
            transform.Rotate(Vector3.up * inputManager.lookInput.x * cameraManager.sensitivity);

        else if (cameraManager.currentView == ViewType.ThirdPerson &&
            moveDir.sqrMagnitude > 0.001f && !player.isAimLocked && networkCharacterController.Grounded)
        {
            Debug.Log("3인칭 회전");
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 중력 적용
    /// </summary>
    public Vector3 Gravity()
    {
        if (networkCharacterController.Grounded && velocity.y < 0)
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

    public void ChangeIdleState()
    {
        canMove = false;
        ChangeState(idleState);
    }
}
