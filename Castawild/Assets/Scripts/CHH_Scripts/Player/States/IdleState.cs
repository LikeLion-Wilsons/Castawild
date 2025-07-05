public class IdleState : MovementBaseState
{
    public IdleState(MovementStateManager _movement, PlayerInputManager _inputManager)
        : base(_movement, _inputManager)
    {
    }

    public override void EnterState()
    {
    }

    public override void UpdateState()
    {
        if (movement.inputManager.moveInput.magnitude > 0.1f)
        {
            if (movement.inputManager.sprintAction.IsPressed())
                movement.SwitchState(movement.runState);
            else
                movement.SwitchState(movement.walkState);
        }
        if (movement.inputManager.crouchAction.WasPressedThisFrame())
            movement.SwitchState(movement.crouchState);
    }
}