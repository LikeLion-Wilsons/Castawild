using UnityEngine;

public class MovementStateManager : MonoBehaviour
{
    #region Components
    [HideInInspector] public Animator anim;
    [SerializeField] private Transform cameraTransform;
    #endregion

    #region States
    public MovementBaseState currentState;
    public IdleState idleState;
    public WalkState walkState;
    public RunState runState;
    public CrouchState crouchState;
    #endregion

    #region Movement
    public float currentMoveSpeed;
    public float walkSpeed = 3;
    public float runSpeed = 7;
    public float crouchSpeed = 2;
    [HideInInspector] public Vector3 dir;

    [HideInInspector] public PlayerInputManager inputManager;
    [SerializeField] private CharacterController controller;
    #endregion

    #region GoundCheck
    [SerializeField] float groundYOffset;
    [SerializeField] LayerMask groundMask;
    private Vector3 spherePos;
    #endregion

    #region Gravity
    [SerializeField] float gravity = -9.81f;
    Vector3 velocity;
    #endregion

    #region Animation
    [SerializeField] private float animationLerpSpeed = 10f;
    private float currentHorizontal;
    private float currentVertical;
    #endregion

    private void Awake()
    {
        InitializeComponents();
        InitializeStates();
        SwitchState(idleState);
    }

    private void InitializeComponents()
    {
        controller = GetComponent<CharacterController>();
        inputManager = GetComponent<PlayerInputManager>();
        anim = GetComponentInChildren<Animator>();
    }

    private void InitializeStates()
    {
        idleState = new IdleState(this, inputManager);
        walkState = new WalkState(this, inputManager);
        runState = new RunState(this, inputManager);
        crouchState = new CrouchState(this, inputManager);

        currentState = idleState;
    }

    private void Update()
    {
        inputManager.HandleMovementInput();

        GetDirectionAndMove();
        Gravity();

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
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        dir = forward * inputManager.moveInput.y + right * inputManager.moveInput.x;
        controller.Move(dir * currentMoveSpeed * Time.deltaTime);

        if (dir.sqrMagnitude > 0.001f)
        {
            Vector3 lookDirection = cameraTransform.forward;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }

    private void Gravity()
    {
        if (IsGrounded())
            velocity.y += gravity * Time.deltaTime;
        else if (velocity.y < 0)
            velocity.y = -2f;

        controller.Move(velocity * Time.deltaTime);
    }

    private bool IsGrounded()
    {
        spherePos = new Vector3(transform.position.x, transform.position.y - groundYOffset, transform.position.z);
        if (Physics.CheckSphere(spherePos, controller.radius - 0.05f, groundMask))
            return true;
        return false;
    }

    public void SwitchState(MovementBaseState newState)
    {
        currentState = newState;
        currentState.EnterState();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spherePos, controller.radius - 0.05f);
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
