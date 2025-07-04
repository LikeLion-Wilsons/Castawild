public class IdleState : MovementBaseState
{
    public override void EnterState(MovementStateManager movement)
    {
    }

    public override void UpdateState(MovementStateManager movement)
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