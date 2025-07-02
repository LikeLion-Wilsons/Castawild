using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Player player;
    private Rigidbody rigid;
    private NavMeshAgent agent;

    [SerializeField] private InputActionAsset inputActions;

    [Header("Camera")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform cameraRoot;

    [Header("Move")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float crouchSpeed = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float airMoveSpeed = 3f;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lookAction;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float pitch;

    private bool isJumping;
    private bool isCursorLocked = false;

    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Awake()
    {
        InitializeComponents();
        InitializeInputActions();
    }

    private void InitializeInputActions()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        lookAction = InputSystem.actions.FindAction("Look");
    }

    private void InitializeComponents()
    {
        player = GetComponent<Player>();
        rigid = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        Camera cam = Camera.main;
        if (cam.transform.parent != cameraRoot)
        {
            cam.transform.SetParent(cameraRoot);
            cam.transform.localPosition = Vector3.zero;
            cam.transform.localRotation = Quaternion.identity;
        }
    }

    private void Update()
    {
        // 게임 포커스가 사라지면 커서 해제
        if (!Application.isFocused && isCursorLocked)
            UnlockCursor();

        // Game 창이 포커스된 상태에서 클릭 시 커서 잠금
        if (!isCursorLocked && Application.isFocused && Mouse.current.leftButton.wasPressedThisFrame)
            LockCursor();

        // 커서 잠겨 있을 때만 회전 처리
        if (isCursorLocked)
        {
            RotateCamera();
            MovePlayer();
        }

        // ESC 눌렀을 때 해제
        if (isCursorLocked && Keyboard.current.escapeKey.wasPressedThisFrame)
            UnlockCursor();
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
    }

    private void RotateCamera()
    {
        lookInput = lookAction.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void MovePlayer()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        if (!isJumping)
        {
            Move(moveInput);

            if (jumpAction.WasPressedThisFrame())
                StartJump();
        }
    }

    private void Move(Vector2 input)
    {
        Vector3 direction = GetCameraRelativeDirection(input);
        Vector3 target = transform.position + direction * 0.5f;

        agent.SetDestination(target);
    }

    private void StartJump()
    {
        isJumping = true;

        agent.enabled = false;
        rigid.isKinematic = false;
        rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        Jump();
    }

    private void Jump()
    {
        if (isJumping)
        {
            Vector3 moveDir = GetCameraRelativeDirection(moveInput);
            Vector3 velocity = rigid.linearVelocity;

            velocity.x = moveDir.x * airMoveSpeed;
            velocity.z = moveDir.z * airMoveSpeed;

            rigid.linearVelocity = velocity;
        }
    }

    private Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        Vector3 camForward = cameraRoot.forward;
        Vector3 camRight = cameraRoot.right;

        camForward.y = 0;
        camRight.y = 0;

        return (camRight * input.x + camForward * input.y).normalized;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isJumping && collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;

            rigid.isKinematic = true;
            agent.enabled = true;

            agent.Warp(transform.position);
        }
    }
}
