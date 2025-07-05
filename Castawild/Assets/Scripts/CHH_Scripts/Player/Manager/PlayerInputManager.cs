using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    private CwPlayer player;

    [HideInInspector] public InputAction moveAction;
    [HideInInspector] public InputAction jumpAction;
    [HideInInspector] public InputAction crouchAction;
    [HideInInspector] public InputAction lookAction;
    [HideInInspector] public InputAction zoomAction;
    [HideInInspector] public InputAction sprintAction;
    [HideInInspector] public InputAction attackAction;

    public Vector2 moveInput { get; private set; }
    public float moveAmount;
    public float verticalInput;
    public float horizontalInput;

    public Vector2 lookInput { get; private set; }
    public Vector2 zoomInput;

    public bool isCursorLocked = false;

    public Action cursorLocked;
    public Action cursorUnLocked;

    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        InitializeInputActions();

        player = GetComponent<CwPlayer>();
    }

    private void InitializeInputActions()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        crouchAction = InputSystem.actions.FindAction("Crouch");
        lookAction = InputSystem.actions.FindAction("Look");
        zoomAction = InputSystem.actions.FindAction("Zoom");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        attackAction = InputSystem.actions.FindAction("Attack");
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
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;

        cursorLocked?.Invoke();
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;

        cursorUnLocked?.Invoke();
    }

    /// <summary>
    /// 입력 처리
    /// </summary>
    public void HandleAllInputs()
    {
        HandleMovementInput();
        HandleJumpingInput();

        // TODO
        //HandleActionInput();
    }

    private void HandleMovementInput()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        verticalInput = moveInput.y;
        horizontalInput = moveInput.x;
    }

    private void HandleJumpingInput()
    {

    }

    public void HandleCameraInput()
    {
        lookInput = lookAction.ReadValue<Vector2>();
        zoomInput = zoomAction.ReadValue<Vector2>();
    }
}
