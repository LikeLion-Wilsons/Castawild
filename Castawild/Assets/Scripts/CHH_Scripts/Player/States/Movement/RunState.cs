public class RunState : MovementBaseState
{
    public RunState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.anim.SetBool("Running", true);
        movementManager.currentMoveSpeed = movementManager.runSpeed;
    }

    public override void UpdateState()
    {
        if (movementManager.inputManager.sprintAction.WasReleasedThisFrame())
            ExitState(movementManager.walkState);
        else if (movementManager.dir.magnitude < 0.1f)
            ExitState(movementManager.idleState);
    }

    void ExitState(MovementBaseState state)
    {
        movementManager.anim.SetBool("Running", false);
        movementManager.SwitchState(state);
    }
}