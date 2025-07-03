using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;

public class PlayerInputController : MonoBehaviour
{
    public InputActionAsset inputActions;

    [Header("Camera")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform cameraRoot;

    [HideInInspector] public InputAction moveAction;
    [HideInInspector] public InputAction jumpAction;
    [HideInInspector] public InputAction lookAction;
    [HideInInspector] public InputAction sprintAction;
    [HideInInspector] public InputAction attackAction;

    private float pitch;
    public Vector2 moveInput { get; private set; }
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
        InitializeComponents();
    }

    private void InitializeInputActions()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        lookAction = InputSystem.actions.FindAction("Look");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        attackAction = InputSystem.actions.FindAction("Attack");
    }

    private void InitializeComponents()
    {
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
        moveInput = moveAction.ReadValue<Vector2>();

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
}
