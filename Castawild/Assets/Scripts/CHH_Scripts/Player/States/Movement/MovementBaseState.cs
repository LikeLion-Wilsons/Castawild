public abstract class MovementBaseState : BaseState
{
    protected MovementStateManager movementManager;

    public MovementBaseState(MovementStateManager _movementManager)
    {
        movementManager = _movementManager;
    }
}