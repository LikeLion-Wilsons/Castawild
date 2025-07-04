using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform thirdPersonCameraTransform;
    private Transform thirdPersonCamera;
    private CwPlayer player;
    private PlayerInputManager inputManager;
    [SerializeField] private bool shouldFaceMoveDirection = false;

    private Vector3 moveDirection;
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
        moveSpeed = player.walkSpeed;
    }

    private void InitializeComponents()
    {
        thirdPersonCamera = Camera.main.transform;
        player = GetComponent<CwPlayer>();
        inputManager = GetComponent<PlayerInputManager>();
    }

    private void Update()
    {
        SetMoveState();

        if (inputManager.jumpAction.WasPressedThisFrame())
            Jump();

        // 떨어질 때
        if (player.rigid.linearVelocity.y < 0f && !player.isGround)
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
        if (!player.canJump)
            return;

        player.canJump = false;
        player.ChangeStateMachine(player.inAirState);
    }

    private void SetMoveState()
    {
        if (inputManager.sprintAction.WasPressedThisFrame() && isSprintToggle)
        {
            if (player.moveSpeedState == MoveSpeedState.Run)
                ChangeMoveSpeed(MoveSpeedState.Walk);
            else
                ChangeMoveSpeed(MoveSpeedState.Run);
        }

        else if (inputManager.sprintAction.IsPressed() && !isSprintToggle)
        {
            ChangeMoveSpeed(MoveSpeedState.Run);
        }

        else if (inputManager.sprintAction.WasReleasedThisFrame() && !isSprintToggle)
        {
            ChangeMoveSpeed(MoveSpeedState.Walk);
        }
    }

    private void ChangeMoveSpeed(MoveSpeedState _moveState)
    {
        player.ChangePlayerMoveState(_moveState);
        switch (_moveState)
        {
            case MoveSpeedState.Run:
                moveSpeed = player.runSpeed;
                break;
            case MoveSpeedState.Walk:
                moveSpeed = player.walkSpeed;
                break;
        }
    }

    public void HandleAllMovement()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        moveDirection = thirdPersonCamera.forward * inputManager.verticalInput;
        moveDirection = moveDirection + thirdPersonCamera.right * inputManager.horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;
        moveDirection = moveDirection * moveSpeed;

        Vector3 movementVelocity = moveDirection;
        player.rigid.linearVelocity = movementVelocity;
    }

    private void HandleRotation()
    {
        Vector3 targetDirection = Vector3.zero;

        targetDirection = thirdPersonCamera.transform.forward * inputManager.verticalInput;
        targetDirection = targetDirection + thirdPersonCamera.transform.right * inputManager.horizontalInput;
        targetDirection = targetDirection.normalized;
        targetDirection.y = 0f;

        if (targetDirection == Vector3.zero)
            targetDirection = transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, player.rotationSpeed * Time.fixedDeltaTime);

        transform.rotation = playerRotation;
    }
}
