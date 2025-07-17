
public class CrouchState : MovementBaseState
{
    public CrouchState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.networkManager.CurrentMoveType = MoveAnimationType.CrouchIdle;
        movementManager.currentMoveSpeed = movementManager.crouchSpeed;
        movementManager.player.currentMoveType = MoveType.Crouch;
    }

    public override void UpdateState()
    {
        if (movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
            movementManager.networkManager.CurrentMoveType = MoveAnimationType.CrouchWalk;
        else
            movementManager.networkManager.CurrentMoveType = MoveAnimationType.CrouchWalk;

        if (movementManager.input.IsDown(PlayerNetworkInputData.sprintInput))
            movementManager.ChangeState(movementManager.runState);
        else if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.crouchInput))
        {
            if (!movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
                movementManager.ChangeState(movementManager.idleState);
            else
                movementManager.ChangeState(movementManager.walkState);
        }
    }

    public override void ExitState()
    {
    }
}