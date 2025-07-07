public class IdleState : MovementBaseState
{
    public IdleState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
    }

    public override void UpdateState()
    {
        if (movementManager.inputManager.moveInput.magnitude > 0.1f)
        {
            if (movementManager.inputManager.sprintAction.IsPressed())
                movementManager.SwitchState(movementManager.runState);
            else
                movementManager.SwitchState(movementManager.walkState);
        }
        if (movementManager.inputManager.crouchAction.WasPressedThisFrame())
            movementManager.SwitchState(movementManager.crouchState);
    }
}