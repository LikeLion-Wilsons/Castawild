public abstract class MovementBaseState : BaseState
{
    protected MovementStateManager movementManager;

    public MovementBaseState(MovementStateManager _movementManager)
    {
        movementManager = _movementManager;
    }
    public override void ExitState()
    {
        movementManager.IsAnimationFinished = false;
    }
}