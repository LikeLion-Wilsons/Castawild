using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CwPlayer player;
    private PlayerInputController inputController;

    [Header("Move")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float crouchSpeed = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float airMoveSpeed = 3f;
    public bool isSprintToggle = true;

    private float moveSpeed;

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
        player = GetComponent<CwPlayer>();
        inputController = GetComponent<PlayerInputController>();
    }

    private void Update()
    {
        SetMoveState();

        if (inputController.jumpAction.WasPressedThisFrame())
            Jump();

        // 떨어질 때
        if (player.rigid.linearVelocity.y < 0f && !player.isGrounded)
        {
            player.isFalling = true;
            player.anim.SetBool("isFalling", player.isFalling);

            // 땅에서 떨어질 때
            if (player.stateMachine.currentState != player.inAirState)
                player.ChangeStateMachine(player.inAirState);
        }
    }

    private void Jump()
    {
        if (player.stateMachine.currentState == player.inAirState)
            return;

        player.isJumping = true;
        player.ChangeStateMachine(player.inAirState);
    }

    private void SetMoveState()
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
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (inputController.moveInput.magnitude > 0f)
        {
            if (player.stateMachine.currentState == player.idleState)
                player.ChangeStateMachine(player.moveState);

            Vector3 forwardMove = transform.forward * inputController.moveInput.y;
            Vector3 rightMove = transform.right * inputController.moveInput.x;

            Vector3 moveDir = (forwardMove + rightMove).normalized;
            player.rigid.MovePosition(player.rigid.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private void JumpAnimationTrigger() => player.rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
}
