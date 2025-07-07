using UnityEngine;

public class AimStateManger : MonoBehaviour
{
    private PlayerInputManager inputManager;
    [HideInInspector] public MovementStateManager movementManager;
    [HideInInspector] public Animator anim;

    #region States
    public AimBaseState currentState;
    public AttackState attackState;
    public AimState aimState;
    #endregion

    [SerializeField] float mouseSense = 1f;
    [SerializeField] private Transform camFollowPos;
    float xAxis, yAxis;

    private void Awake()
    {
        InitializeComponents();
        InitializeStates();
        SwitchState(attackState);
    }

    private void InitializeComponents()
    {
        inputManager = GetComponent<PlayerInputManager>();
        movementManager = GetComponent<MovementStateManager>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        inputManager.HandleMovementInput();

        xAxis += inputManager.lookInput.y * mouseSense;
        yAxis += inputManager.lookInput.x * mouseSense;
        yAxis = Mathf.Clamp(yAxis, -80f, 80f);

        currentState.UpdateState();
    }

    private void LateUpdate()
    {
        camFollowPos.localEulerAngles = new Vector3(yAxis, camFollowPos.localEulerAngles.y, camFollowPos.localEulerAngles.z);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, xAxis, transform.eulerAngles.z);
    }

    private void InitializeStates()
    {
        attackState = new AttackState(this, inputManager);
        aimState = new AimState(this, inputManager);
    }

    public void SwitchState(AimBaseState newState)
    {
        currentState = newState;
        currentState.EnterState();
    }
}