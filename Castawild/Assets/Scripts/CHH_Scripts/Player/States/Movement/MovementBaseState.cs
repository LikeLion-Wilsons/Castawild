public abstract class MovementBaseState : BaseState
{
    protected MovementStateManager movementManager;

    public override void EnterState()
    {
        if (movementManager.currentState == movementManager.idleState)
            movementManager.CurrentMoveType = MoveType.Idle;
        else if (movementManager.currentState == movementManager.walkState)
            movementManager.CurrentMoveType = MoveType.Walk;
        else if (movementManager.currentState == movementManager.runState)
            movementManager.CurrentMoveType = MoveType.Run;
        else if (movementManager.currentState == movementManager.crouchState)
            movementManager.CurrentMoveType = MoveType.Crouch;
        else if (movementManager.currentState == movementManager.jumpState)
            movementManager.CurrentMoveType = MoveType.Jump;
    }

    public MovementBaseState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
    {
        movementManager = _movementManager;
        inputManager = _inputManager;
    }
}