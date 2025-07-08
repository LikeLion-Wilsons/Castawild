using UnityEngine;

public class AttackStateManager : BaseStateManager
{
    #region Components
    [HideInInspector] public MovementStateManager movementManager;
    [HideInInspector] public PlayerCameraManager cameraManager;
    [HideInInspector] public AnimationTrigger animTrigger;
    #endregion

    #region States
    public AttackIdleState idleState;
    public AttackState attackState;
    public AimState aimState;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        InitComponents();
        InitStates();
    }

    private void InitComponents()
    {
        movementManager = GetComponent<MovementStateManager>();
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
        animTrigger = GetComponentInChildren<AnimationTrigger>();
    }

    private void InitStates()
    {
        idleState = new AttackIdleState(this, inputManager);
        attackState = new AttackState(this, inputManager);
        aimState = new AimState(this, inputManager);

        ChangeState(idleState);
    }

    private void Update()
    {
        inputManager.HandleMovementInput();
        currentState?.UpdateState();
    }
}