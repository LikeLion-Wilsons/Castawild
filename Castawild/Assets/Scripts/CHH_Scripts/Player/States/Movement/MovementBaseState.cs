public abstract class MovementBaseState : BaseState
{
    protected MovementStateManager movementManager;

    public MovementBaseState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
    {
        movementManager = _movementManager;
        inputManager = _inputManager;
    }
}