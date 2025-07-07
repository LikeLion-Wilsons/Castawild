
public abstract class AimBaseState
{
    protected AimStateManger aimManager;
    protected PlayerInputManager inputManager;

    public AimBaseState(AimStateManger _aimManager, PlayerInputManager _inputManager)
    {
        aimManager = _aimManager;
        inputManager = _inputManager;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
}