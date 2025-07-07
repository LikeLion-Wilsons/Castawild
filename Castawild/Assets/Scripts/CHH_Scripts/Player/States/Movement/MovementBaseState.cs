public abstract class MovementBaseState
{
    protected MovementStateManager movementManager;
    protected PlayerInputManager inputManager;

    public MovementBaseState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
    {
        movementManager = _movementManager;
        inputManager = _inputManager;
    }

    public abstract void EnterState();
    public abstract void UpdateState();

}