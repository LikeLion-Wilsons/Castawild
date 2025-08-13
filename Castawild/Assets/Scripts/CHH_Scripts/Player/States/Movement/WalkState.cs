
public class WalkState : MovementBaseState
{
    public WalkState(MovementStateManager _movementManager)
        : base(_movementManager)
    {
    }

    public override void EnterState()
    {
        movementManager.CurrentMoveAnimation = MoveAnimatoinState.Walk;
        movementManager.flagManager.Set(PlayerFlags.Walk);

        movementManager.moveManager.currentMoveSpeed = movementManager.walkSpeed;
        if (movementManager.HasInputAuthority)
            movementManager.cameraManager.walk = true;
    }

    public override void UpdateState()
    {
        // Run
        if (movementManager.input.IsDown(PlayerNetworkInputData.sprintInput) && movementManager.All_CanRun())
            movementManager.Host_ChangeState(MovementState.Run);

        // Crouch
        else if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.crouchInput))
            movementManager.Host_ChangeState(MovementState.Crouch);

        // Idle
        else if (!movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
            movementManager.Host_ChangeState(MovementState.Idle);

        // Jump
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.jumpInput) && movementManager.moveManager.Grounded)
        {
            movementManager.previousState = MovementState.Walk;
            movementManager.Host_ChangeState(MovementState.Jump);
        }
    }

    public override void ExitState()
    {
        movementManager.flagManager.Clear(PlayerFlags.Walk);

        if (movementManager.HasInputAuthority)
            movementManager.cameraManager.walk = false;
    }
}