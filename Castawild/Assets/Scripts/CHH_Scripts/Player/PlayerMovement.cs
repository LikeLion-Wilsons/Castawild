using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Player player;
    private Rigidbody rigid;
    private PlayerInputController inputController;

    [Header("Move")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float crouchSpeed = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float airMoveSpeed = 3f;
    public bool isSprintToggle = true;

    private float moveSpeed;

    [SerializeField] private Transform groundCheck;
    int groundLayer = LayerMask.GetMask("Ground");
    private bool isGround;
    private bool isJumping;

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Awake()
    {
        InitializeComponents();
        moveSpeed = walkSpeed;
    }

    private void InitializeComponents()
    {
        player = GetComponent<Player>();
        rigid = GetComponent<Rigidbody>();
        inputController = GetComponent<PlayerInputController>();
    }

    private void Update()
    {
        if (inputController.sprintAction.WasPressedThisFrame() && isSprintToggle)
        {
            player.ChangePlayerMoveState(MoveState.Run);
            moveSpeed = runSpeed;
        }

        else if (inputController.sprintAction.IsPressed() && !isSprintToggle)
        {
            player.ChangePlayerMoveState(MoveState.Run);
            moveSpeed = walkSpeed;
        }

        else if (inputController.sprintAction.WasReleasedThisFrame())
        {
            player.ChangePlayerMoveState(MoveState.Walk);
            moveSpeed = walkSpeed;
        }

        if (inputController.jumpAction.WasPressedThisFrame())
            Jump();
        isGround = IsGround();
    }


    bool IsGround()
    {
        float rayDistance = 1.1f;
        bool _isGround = Physics.Raycast(groundCheck.position, Vector3.down, rayDistance, groundLayer);

        // 땅에서 떨어짐
        if (!isJumping && isGround && !_isGround)
            player.ChangeStateMachine(player.inAirState);

        return _isGround;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (inputController.moveInput.magnitude > 0f)
        {
            player.ChangeStateMachine(player.moveState);
            rigid.MovePosition(rigid.position + transform.forward * inputController.moveInput.y * moveSpeed * Time.fixedDeltaTime);
        }
        else if (inputController.moveInput.magnitude == 0f)
            player.ChangeStateMachine(player.idleState);
    }

    private void Jump()
    {
        if (!isGround)
            return;

        isJumping = true;
        rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        player.anim.SetBool("isJumping", true);
        player.ChangeStateMachine(player.inAirState);
    }
}
