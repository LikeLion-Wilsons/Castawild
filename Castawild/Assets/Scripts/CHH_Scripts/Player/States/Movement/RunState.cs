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
        if (movementManager.input.IsUp(PlayerNetworkInputData.sprintInput))
            movementManager.ChangeState(movementManager.walkState);

        // Idle
        else if (!inputManager.MoveInputDectected())
            movementManager.ChangeState(movementManager.idleState);

        // Jump
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.jumpInput) && movementManager.canJump)
        {
            movementManager.canJump = false;
            movementManager.previousState = this;
            movementManager.ChangeState(movementManager.jumpState);
        }
    }

    public override void ExitState()
    {
        movementManager.anim.SetBool("Running", false);
    }
}