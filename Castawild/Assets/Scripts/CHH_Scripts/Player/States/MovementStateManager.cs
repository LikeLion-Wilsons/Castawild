using UnityEngine;

public class MovementStateManager : MonoBehaviour
{
    [HideInInspector] public Animator anim;

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

    MovementBaseState currentState;
    public IdleState idleState = new IdleState();
    public WalkState walkState = new WalkState();
    public RunState runState = new RunState();
    public CrouchState crouchState = new CrouchState();

    [SerializeField] private float animationLerpSpeed = 10f;
    float currentHorizontal;
    float currentVertical;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        inputManager = GetComponent<PlayerInputManager>();
        anim = GetComponentInChildren<Animator>();
        SwitchState(idleState);
    }

    private void Update()
    {
        GetDirectionAndMove();
        Gravity();

        currentState.UpdateState(this);
    }

    private void GetDirectionAndMove()
    {
        inputManager.HandleAllInputs();
        currentHorizontal = Mathf.Lerp(currentHorizontal, inputManager.horizontalInput, Time.deltaTime * animationLerpSpeed);
        currentVertical = Mathf.Lerp(currentVertical, inputManager.verticalInput, Time.deltaTime * animationLerpSpeed);

        anim.SetFloat("Horizontal", currentHorizontal);
        anim.SetFloat("Vertical", currentVertical);
        dir = transform.forward * inputManager.verticalInput + transform.right * inputManager.horizontalInput;
        controller.Move(dir * currentMoveSpeed * Time.deltaTime);
    }

    void Gravity()
    {
        if (IsGrounded())
            velocity.y += gravity * Time.deltaTime;
        else if (velocity.y < 0)
            velocity.y = -2f;

        controller.Move(velocity * Time.deltaTime);
    }

    bool IsGrounded()
    {
        spherePos = new Vector3(transform.position.x, transform.position.y - groundYOffset, transform.position.z);
        if (Physics.CheckSphere(spherePos, controller.radius - 0.05f, groundMask))
            return true;
        return false;
    }

    public void SwitchState(MovementBaseState newState)
    {
        currentState = newState;
        currentState.EnterState(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spherePos, controller.radius - 0.05f);
    }
}

