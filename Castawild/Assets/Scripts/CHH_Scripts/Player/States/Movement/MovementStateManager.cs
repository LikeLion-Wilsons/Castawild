using UnityEngine;

public class MovementStateManager : BaseStateManager
{
    #region Conponent
    [HideInInspector] public ToolStateManager toolStateManager;
    //[SerializeField] private CharacterController controller;
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
    [HideInInspector] public bool canMove = true;
    public float currentMoveSpeed;
    public float airSpeedMuliplier = 0.7f;
    public float walkSpeed = 3f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;
    public float rotationSpeed = 10f;
    [HideInInspector] public Vector3 dir;
    [HideInInspector] public bool canJump = true;
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
    private float currentHorizontal;
    private float currentVertical;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        InitComponents();
        InitStates();
    }

    private void InitComponents()
    {
        //controller = GetComponent<CharacterController>();
        toolStateManager = GetComponent<ToolStateManager>();
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

    private void Update()
    {
        UpdateMoveAnimation();
        currentState.UpdateState();
    }

    private void UpdateMoveAnimation()
    {
        currentHorizontal = Mathf.Lerp(currentHorizontal, inputManager.horizontalInput, Time.deltaTime * animationLerpSpeed);
        currentVertical = Mathf.Lerp(currentVertical, inputManager.verticalInput, Time.deltaTime * animationLerpSpeed);

        anim.SetFloat("Horizontal", currentHorizontal);
        anim.SetFloat("Vertical", currentVertical);
        anim.SetBool("Falling", !IsGrounded());
    }




    public float sensitivity = 1.5f;
    public float maxXRotation = 80f;
    public float minXRotation = -80f;

    private float xRotation = 0f;
    /// <summary>
    /// 움직이는 방향
    /// </summary>
    public Vector3 GetMoveDir(bool hasInputAuthority)
    {
        if (!canMove)
            return Vector3.zero;

        Vector3 forward;
        Vector3 right;

        if (cameraManager.currentView == ViewType.FirstPerson && hasInputAuthority)
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

        dir = forward * inputManager.moveInput.y + right * inputManager.moveInput.x;

        if ((hasInputAuthority && cameraManager.currentView == ViewType.ThirdPerson)
            || !hasInputAuthority)
            RotateToward(dir);

        else if (hasInputAuthority && cameraManager.currentView == ViewType.FirstPerson && inputManager.isCursorLocked)
            transform.Rotate(Vector3.up * inputManager.lookInput.x * cameraManager.sensitivity);

        return dir;
    }

    private void RotateToward(Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude > 0.001f && !player.isAimLocked && IsGrounded())
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
        if (IsGrounded() && velocity.y < 0)
            velocity.y = -1f;
        else
            velocity.y += gravity * fallMultiplier * Time.fixedDeltaTime;

        return velocity;
    }

    /// <summary>
    /// 땅 체크
    /// </summary>
    public bool IsGrounded()
    {
        spherePos = new Vector3(transform.position.x, transform.position.y - groundYOffset, transform.position.z);
        if (Physics.CheckSphere(spherePos, groundCheckRadius, groundMask))
            return true;
        return false;
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
