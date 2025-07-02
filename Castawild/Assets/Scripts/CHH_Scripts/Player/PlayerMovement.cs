using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Player player;
    private Rigidbody rigid;

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

        // ESC 눌렀을 때 해제
        if (isCursorLocked && Keyboard.current.escapeKey.wasPressedThisFrame)
            UnlockCursor();

        // 커서 잠겨 있을 때만 회전 처리
        if (isCursorLocked)
            RotateCamera();

        moveInput = moveAction.ReadValue<Vector2>();
        if (jumpAction.WasPressedThisFrame())
            Jump();
    }

    private void FixedUpdate()
    {
        Move();
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

    private void Move()
    {
        rigid.MovePosition(rigid.position + transform.forward * moveInput.y * walkSpeed * Time.fixedDeltaTime);
    }

    private void Jump()
    {
        rigid.AddForceAtPosition(new Vector3(0f, jumpForce, 0f), Vector3.up, ForceMode.Impulse);
    }
}
