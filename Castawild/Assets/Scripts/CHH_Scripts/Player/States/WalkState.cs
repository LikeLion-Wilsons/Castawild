public class WalkState : MovementBaseState
{
    public override void EnterState(MovementStateManager movement)
    {
        movement.anim.SetBool("Walking", true);
        movement.currentMoveSpeed = movement.walkSpeed;
    }

    public override void UpdateState(MovementStateManager movement)
    {
        if (movement.inputManager.sprintAction.IsPressed())
            ExitState(movement, movement.runState);
        else if (movement.inputManager.crouchAction.WasPressedThisFrame())
            ExitState(movement, movement.crouchState);
        else if (movement.dir.magnitude < 0.1f)
            ExitState(movement, movement.idleState);
    }

    void ExitState(MovementStateManager movement, MovementBaseState state)
    {
        movement.anim.SetBool("Walking", false);
        movement.SwitchState(state);
    }
}