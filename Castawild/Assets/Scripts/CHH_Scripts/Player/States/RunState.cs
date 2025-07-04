public class RunState : MovementBaseState
{
    public override void EnterState(MovementStateManager movement)
    {
        movement.anim.SetBool("Running", true);
        movement.currentMoveSpeed = movement.runSpeed;
    }

    public override void UpdateState(MovementStateManager movement)
    {
        if (movement.inputManager.sprintAction.WasReleasedThisFrame())
            ExitState(movement, movement.walkState);
        else if (movement.dir.magnitude < 0.1f)
            ExitState(movement, movement.idleState);
    }

    void ExitState(MovementStateManager movement, MovementBaseState state)
    {
        movement.anim.SetBool("Running", false);
        movement.SwitchState(state);
    }
}