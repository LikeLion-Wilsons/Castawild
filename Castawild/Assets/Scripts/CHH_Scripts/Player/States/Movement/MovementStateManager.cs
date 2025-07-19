using Fusion;
using UnityEngine;

public class MovementStateManager : MonoBehaviour
{
    #region Conponent
    [HideInInspector] public Animator anim;
    [HideInInspector] public PlayerInputManager inputManager;
    [HideInInspector] public PlayerCameraManager cameraManager;
    [HideInInspector] public CwPlayer player;
    [HideInInspector] public PlayerNetworkManager networkManager;
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
    private float currentHorizontal;
    private float currentVertical;
    #endregion

    #region Network
    public PlayerNetworkInputData input { get; private set; }
    public NetworkButtons prevInputButtons;
    #endregion

    protected void Awake()
    {
        InitComponents();
        InitStates();
    }

    private void InitComponents()
    {
        toolStateManager = GetComponent<ToolStateManager>();
        networkCharacterController = GetComponent<NetworkCharacterControllerCustom>();
        anim = GetComponentInChildren<Animator>();
        inputManager = GetComponent<PlayerInputManager>();
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
        player = GetComponent<CwPlayer>();
        networkManager = GetComponent<PlayerNetworkManager>();
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

    public void ChangeState(BaseState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }

    public void UpdateMoveAnimation()
    {
        //currentHorizontal = Mathf.Lerp(currentHorizontal, inputManager.horizontalInput, Time.deltaTime * animationLerpSpeed);
        //currentVertical = Mathf.Lerp(currentVertical, inputManager.verticalInput, Time.deltaTime * animationLerpSpeed);

        //anim.SetFloat("Horizontal", currentHorizontal);
        //anim.SetFloat("Vertical", currentVertical);

        anim.SetBool("Walking", false);
        anim.SetBool("Running", false);
        anim.SetBool("Crouching", false);
        anim.SetBool("Falling", false);

        switch (networkManager.CurrentMoveType)
        {
            case MoveAnimationType.Walk:
                anim.SetBool("Walking", true);
                break;
            case MoveAnimationType.Run:
                anim.SetBool("Running", true);
                break;
            case MoveAnimationType.CrouchIdle:
                anim.SetBool("Crouching", true);
                break;
            case MoveAnimationType.CrouchWalk:
                anim.SetBool("Crouching", true);
                anim.SetBool("Walking", true);
                break;
            case MoveAnimationType.IdleJump:
                if (!isTriggerSet)
                    anim.SetTrigger("IdleJump");
                isTriggerSet = true;
                break;
            case MoveAnimationType.RunJump:
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

        if (isLocalPlayer && cameraManager.CurrentView == ViewType.FirstPerson)
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
        if (cameraManager.CurrentView == ViewType.FirstPerson && inputManager.isCursorLocked)
            transform.Rotate(Vector3.up * inputManager.lookInput.x * cameraManager.sensitivity);

        else if (cameraManager.CurrentView == ViewType.ThirdPerson &&
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

    public bool isTriggerSet = false;




    public void HandleState(PlayerNetworkInputData input)
    {
        SetInput(input);

        currentState.UpdateState();
        currentState.UpdateState();

        SetPrevInputButton(input.Buttons);
    }

    public Vector3 GetMoveDir(Vector2 moveInput)
    {
        if (!canMove)
            return Vector3.zero;

        Vector3 forward;
        Vector3 right;

        forward = transform.forward;
        right = transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return forward * moveInput.y + right * moveInput.x;
    }

    public void SetInput(PlayerNetworkInputData inputData) => input = inputData;
    public void SetPrevInputButton(NetworkButtons _prevInputButtons) => prevInputButtons = _prevInputButtons;
}
