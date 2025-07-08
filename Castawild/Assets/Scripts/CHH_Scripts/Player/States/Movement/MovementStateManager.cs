using UnityEngine;
using UnityEngine.EventSystems;

public class MovementStateManager : BaseStateManager
{
    #region Conponent
    [SerializeField] private CharacterController controller;
    #endregion

    #region States
    public MovementBaseState previousState;
    public MovementBaseState currentState;
    public IdleState idleState;
    public WalkState walkState;
    public RunState runState;
    public JumpState jumpState;
    public CrouchState crouchState;
    #endregion

    #region Movement
    public float currentMoveSpeed;
    public float rotationSpeed;
    public float walkSpeed = 3f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;
    [HideInInspector] public Vector3 dir;
    #endregion

    #region GoundCheck
    [SerializeField] private float groundYOffset;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundMask;
    private Vector3 spherePos;
    #endregion

    #region Gravity
    public float gravity = -9.81f;
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
        ChangeState(idleState);
    }

    private void InitComponents()
    {
        controller = GetComponent<CharacterController>();
    }

    private void InitStates()
    {
        idleState = new IdleState(this, inputManager);
        walkState = new WalkState(this, inputManager);
        runState = new RunState(this, inputManager);
        crouchState = new CrouchState(this, inputManager);
        jumpState = new JumpState(this, inputManager);

        currentState = idleState;
    }

    private void Update()
    {
        inputManager.HandleMovementInput();

        GetDirectionAndMove();
        Gravity();
        Falling();

        currentState.UpdateState();
    }

    private void GetDirectionAndMove()
    {
        UpdateMoveAnimation();
        HandleMovement();
    }

    private void UpdateMoveAnimation()
    {
        currentHorizontal = Mathf.Lerp(currentHorizontal, inputManager.horizontalInput, Time.deltaTime * animationLerpSpeed);
        currentVertical = Mathf.Lerp(currentVertical, inputManager.verticalInput, Time.deltaTime * animationLerpSpeed);

        anim.SetFloat("Horizontal", currentHorizontal);
        anim.SetFloat("Vertical", currentVertical);
    }

    private void HandleMovement()
    {
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        dir = forward * inputManager.moveInput.y + right * inputManager.moveInput.x;
        controller.Move(dir * currentMoveSpeed * Time.deltaTime);

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

            dir *= currentMoveSpeed;
            controller.Move(dir * Time.deltaTime);


            //Vector3 lookDirection = cam.transform.forward;
            //lookDirection.y = 0f;

            //if (lookDirection.sqrMagnitude > 0.001f)
            //{
            //    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            //    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            //}
        }
    }

    private void Gravity()
    {
        if (IsGrounded() && velocity.y < 0)
            velocity.y = -1f;
        else
            velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
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

    void Falling() => anim.SetBool("Falling", !IsGrounded());

    public void ChangeState(MovementBaseState newState)
    {
        currentState = newState;
        currentState.EnterState();
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
