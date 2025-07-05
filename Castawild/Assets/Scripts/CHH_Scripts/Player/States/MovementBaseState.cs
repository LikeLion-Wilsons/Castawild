public abstract class MovementBaseState
{
    protected MovementStateManager movement;
    protected PlayerInputManager inputManager;
    public MovementBaseState(MovementStateManager _movement, PlayerInputManager _inputManager)
    {
        movement = _movement;
        inputManager = _inputManager;
    }

    public abstract void EnterState();
    public abstract void UpdateState();

}