using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    #region Input Action
    [SerializeField] private InputActionAsset inputActions;

    [HideInInspector] public InputAction moveAction;
    [HideInInspector] public InputAction jumpAction;
    [HideInInspector] public InputAction crouchAction;
    [HideInInspector] public InputAction lookAction;
    [HideInInspector] public InputAction zoomAction;
    [HideInInspector] public InputAction aimAction;
    [HideInInspector] public InputAction sprintAction;
    [HideInInspector] public InputAction toolAction;

    public Vector2 moveInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    [HideInInspector] public Vector2 zoomInput;
    public bool aimInput { get; private set; }
    public bool attackInput { get; private set; }

    [HideInInspector] public float verticalInput;
    [HideInInspector] public float horizontalInput;
    #endregion

    #region Cursor
    [HideInInspector] public bool isCursorLocked = false;
    public Action cursorLocked;
    public Action cursorUnLocked;
    #endregion 

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
        InitInputActions();
    }

    private void InitInputActions()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        crouchAction = InputSystem.actions.FindAction("Crouch");
        lookAction = InputSystem.actions.FindAction("Look");
        zoomAction = InputSystem.actions.FindAction("Zoom");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        aimAction = InputSystem.actions.FindAction("Aim");
        toolAction = InputSystem.actions.FindAction("Attack");
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

    public void HandleMovementInput()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        verticalInput = moveInput.y;
        horizontalInput = moveInput.x;
    }

    /// <summary>
    /// 카메라 입력 Update
    /// </summary>
    public void HandleCameraInput()
    {
        lookInput = lookAction.ReadValue<Vector2>();
        zoomInput = zoomAction.ReadValue<Vector2>();
    }
}
