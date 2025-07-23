public class RunState : MovementBaseState
{
    public RunState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.networkManager.CurrentMoveState = MoveAnimationState.Run;
        movementManager.currentMoveSpeed = movementManager.runSpeed;
        movementManager.player.currentMoveType = MoveType.Run;
    }

    public override void UpdateState()
    {
        // Walk
        if (movementManager.input.IsUp(PlayerNetworkInputData.sprintInput))
            movementManager.ChangeState(movementManager.walkState);

        // Idle
        else if (!movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
            movementManager.ChangeState(movementManager.idleState);

        // Crouch
        else if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.crouchInput))
            movementManager.ChangeState(movementManager.crouchState);

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
    }
}