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
    [HideInInspector] public CinemachineCamera cineCam;

    #region States
    public AttackBaseState currentState;
    public AttackIdleState idleState;
    public AttackState attackState;
    public AimState aimState;
    #endregion

    public float aimFov = 40f;
    public float mouseSense = 0.5f;

    [HideInInspector] public float throwFov;
    [HideInInspector] public float currentFov;

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
        anim = GetComponentInChildren<Animator>();
        animTrigger = GetComponentInChildren<AnimationTrigger>();
        cineCam = GetComponentInChildren<CinemachineCamera>();
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
        //transform.eulerAngles = new Vector3(transform.eulerAngles.x, xAxis, transform.eulerAngles.z);
    }

    public void ChangeState(AttackBaseState newState)
    {
        currentState.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
}