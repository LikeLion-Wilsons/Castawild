public abstract class BaseState
{
    protected PlayerInputManager inputManager;

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
}