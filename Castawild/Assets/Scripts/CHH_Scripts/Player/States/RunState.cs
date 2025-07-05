public class RunState : MovementBaseState
{
    public RunState(MovementStateManager _movement, PlayerInputManager _inputManager)
        : base(_movement, _inputManager)
    {
    }

    public override void EnterState()
    {
        movement.anim.SetBool("Running", true);
        movement.currentMoveSpeed = movement.runSpeed;
    }

    public override void UpdateState()
    {
        if (movement.inputManager.sprintAction.WasReleasedThisFrame())
            ExitState(movement.walkState);
        else if (movement.dir.magnitude < 0.1f)
            ExitState(movement.idleState);
    }

    void ExitState(MovementBaseState state)
    {
        movement.anim.SetBool("Running", false);
        movement.SwitchState(state);
    }
}