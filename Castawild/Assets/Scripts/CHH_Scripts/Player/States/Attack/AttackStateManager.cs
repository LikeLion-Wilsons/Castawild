using Unity.Cinemachine;
using UnityEngine;

public class AttackStateManager : MonoBehaviour
{
    private PlayerInputManager inputManager;
    [HideInInspector] public MovementStateManager movementManager;
    [HideInInspector] public PlayerCameraManager cameraManager;
    [HideInInspector] public AnimationTrigger animTrigger;
    [SerializeField] private Transform camFollowPos;
    [HideInInspector] public Animator anim;

    #region States
    public AttackBaseState currentState;
    public AttackIdleState idleState;
    public AttackState attackState;
    public AimState aimState;
    #endregion

    public float mouseSense = 1f;
    public float xAxis;
    public float yAxis;

    [HideInInspector] public CinemachineCamera virtualCam;
    public float adsFov = 40f;
    [HideInInspector] public float throwFov;
    [HideInInspector] public float currentFov;
    public float fovSmoothSpeed;


    private void Awake()
    {
        InitializeComponents();
        InitializeStates();
    }

    private void InitializeComponents()
    {
        inputManager = GetComponent<PlayerInputManager>();
        movementManager = GetComponent<MovementStateManager>();
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
        virtualCam = GetComponentInChildren<CinemachineCamera>();
        anim = GetComponentInChildren<Animator>();
        animTrigger = GetComponentInChildren<AnimationTrigger>();
    }

    private void InitializeStates()
    {
        idleState = new AttackIdleState(this, inputManager);
        attackState = new AttackState(this, inputManager);
        aimState = new AimState(this, inputManager);

        currentState = idleState;
    }

    private void Update()
    {
        inputManager.HandleMovementInput();
        currentState?.UpdateState();
    }

    private void LateUpdate()
    {
        camFollowPos.localEulerAngles = new Vector3(yAxis, camFollowPos.localEulerAngles.y, camFollowPos.localEulerAngles.z);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, xAxis, transform.eulerAngles.z);
    }

    public void ChangeState(AttackBaseState newState)
    {
        currentState.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
}