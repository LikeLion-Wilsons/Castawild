using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    private PlayerAnimatorManager animatorManager;

    [Header("Camera")]
    [SerializeField] private float mouseSensitivity = 2f;

    [HideInInspector] public InputAction moveAction;
    [HideInInspector] public InputAction jumpAction;
    [HideInInspector] public InputAction lookAction;
    [HideInInspector] public InputAction sprintAction;
    [HideInInspector] public InputAction attackAction;

    public Vector2 moveInput { get; private set; }
    private float moveAmount;
    public float verticalInput;
    public float horizontalInput;
    public Vector2 lookInput { get; private set; }

    private bool isCursorLocked = false;

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
    }

    private void InitializeInputActions()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        lookAction = InputSystem.actions.FindAction("Look");
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

        // 커서 잠겨 있을 때만 회전 처리
        //if (isCursorLocked)
        //    Rotate();
    }

    /// <summary>
    /// 입력 처리
    /// </summary>
    public void HandleAllInputs()
    {
        HandleMovementInput();

        // TODO 
        //HandleJumpingInput();
        //HandleActionInput();
    }

    private void HandleMovementInput()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        verticalInput = moveInput.y;
        horizontalInput = moveInput.x;

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));
        animatorManager?.UpdateAnimatorValues(0, moveAmount);
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

    public void RegisterAnimatorManger(PlayerAnimatorManager _animManager) => animatorManager = _animManager;
}
