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
        movementManager.player.currentMoveType = MoveType.Run;
    }

    public override void UpdateState()
    {
        // Walk
        if (movementManager.inputManager.sprintAction.WasReleasedThisFrame())
            movementManager.ChangeState(movementManager.walkState);

        // Idle
        else if (movementManager.dir.magnitude < 0.1f)
            movementManager.ChangeState(movementManager.idleState);

        // Jump
        if (inputManager.jumpAction.WasPressedThisFrame())
        {
            movementManager.previousState = this;
            movementManager.ChangeState(movementManager.jumpState);
        }
    }

    public override void ExitState()
    {
        movementManager.anim.SetBool("Running", false);
    }
}