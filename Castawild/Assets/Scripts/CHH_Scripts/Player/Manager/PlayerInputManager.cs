using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInputManager : MonoBehaviour
{
    #region Input Action
    [SerializeField] private InputActionAsset inputActionsRef;
    private InputActionAsset inputActions;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction crouchAction;
    [HideInInspector] public InputAction viewChangeAction;
    [HideInInspector] public InputAction lookAction;
    [HideInInspector] public InputAction zoomAction;
    private InputAction aimAction;
    private InputAction sprintAction;
    private InputAction toolAction;
    public InputAction interactAction;
    private InputAction removeAction;

    [HideInInspector] public Vector2 lookInput;
    [HideInInspector] public Vector2 zoomInput;
    #endregion

    #region Cursor
    public Action<bool> cursorLocked;
    #endregion 

    private Player player;
    private PlayerCameraManager cameraManager;
    private PlayerMoveManager moveManager;


    void Start()
    {
        UIPart.openUI += HandleCursor;
    }

    void OnDestroy()
    {
        UIPart.openUI += HandleCursor;
    }

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
        player = GetComponent<Player>();
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
        moveManager = GetComponent<PlayerMoveManager>();
        InitInputActions();
    }

    private void InitInputActions()
    {
        inputActions = Instantiate(inputActionsRef);

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        crouchAction = InputSystem.actions.FindAction("Crouch");
        viewChangeAction = InputSystem.actions.FindAction("ViewChange");
        lookAction = InputSystem.actions.FindAction("Look");
        zoomAction = InputSystem.actions.FindAction("Zoom");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        aimAction = InputSystem.actions.FindAction("Aim");
        toolAction = InputSystem.actions.FindAction("Attack");
        interactAction = InputSystem.actions.FindAction("Interact");
        removeAction = InputSystem.actions.FindAction("Remove");
    }

    private void Update()
    {
        if (!player.isSpawned || !player.HasInputAuthority)
            return;

        //// 게임 포커스가 사라지면 커서 해제
        //if (!Application.isFocused && player.IsCursorLocked)
        //    UnlockCursor();

        //// Game 창이 포커스된 상태에서 클릭 시 커서 잠금
        //if (!player.IsCursorLocked && Application.isFocused && Mouse.current.leftButton.wasPressedThisFrame && !player.IsUIOpen)
        //    LockCursor();

        //// ESC 눌렀을 때 해제
        //if (player.IsCursorLocked && Keyboard.current.escapeKey.wasPressedThisFrame && !player.IsUIOpen)
        //    UnlockCursor();

        //if (optionUI.gameObject.activeSelf)

        HandleCameraInput();
    }

    private void HandleCursor(bool uiOpen)
    {
        if (uiOpen)
            UnlockCursor();
        else
            LockCursor();
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        player.Client_SetCursorLocked(true);

        cursorLocked?.Invoke(true);
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        player.Client_SetCursorLocked(false);

        cursorLocked?.Invoke(false);
    }

    public PlayerNetworkInputData CollectInput()
    {
        PlayerNetworkInputData inputData = new PlayerNetworkInputData();

        // 버튼은 내부에서 bitmask를 누적시키는 방식이라 따로 여러 번 호출해야함
        inputData.Buttons.Set(PlayerNetworkInputData.moveInput, moveAction.IsPressed());
        inputData.Buttons.Set(PlayerNetworkInputData.jumpInput, jumpAction.IsPressed());
        inputData.Buttons.Set(PlayerNetworkInputData.crouchInput, crouchAction.IsPressed());
        inputData.Buttons.Set(PlayerNetworkInputData.aimInput, aimAction.IsPressed());
        inputData.Buttons.Set(PlayerNetworkInputData.sprintInput, sprintAction.IsPressed());
        inputData.Buttons.Set(PlayerNetworkInputData.toolUseInput, toolAction.IsPressed());
        inputData.Buttons.Set(PlayerNetworkInputData.interactInput, interactAction.IsPressed());
        inputData.Buttons.Set(PlayerNetworkInputData.removeInput, removeAction.IsPressed());

        inputData.moveValue = moveAction.ReadValue<Vector2>();
        inputData = SetMoveDir(inputData);
        inputData.currentView = cameraManager.currentView;

        return inputData;
    }

    private PlayerNetworkInputData SetMoveDir(PlayerNetworkInputData inputData)
    {
        Vector3 forward = Vector3.zero;
        Vector3 right = Vector3.zero;

        if (cameraManager.currentView == ViewType.ThirdPerson)
        {
            forward = cameraManager.CurrenCam.transform.forward;
            right = cameraManager.CurrenCam.transform.right;

            Vector3 camForward = cameraManager.CurrenCam.transform.forward;
            inputData.camForward = new Vector3(camForward.x, 0f, camForward.z);
        }

        if (moveManager.All_CanMoving())
        {
            if (cameraManager.currentView == ViewType.FirstPerson)
            {
                forward = transform.forward;
                right = transform.right;

                inputData.lookValue = lookAction.ReadValue<Vector2>();
            }

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            inputData.moveDir = forward * inputData.moveValue.y + right * inputData.moveValue.x;
        }
        else
            inputData.moveDir = Vector3.zero;

        return inputData;
    }

    private void HandleCameraInput()
    {
        lookInput = lookAction.ReadValue<Vector2>();
        zoomInput = zoomAction.ReadValue<Vector2>();
    }
}
