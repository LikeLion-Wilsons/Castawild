
public abstract class AttackBaseState
{
    protected AttackStateManager attackManager;
    protected PlayerInputManager inputManager;

    public AttackBaseState(AttackStateManager attackManger, PlayerInputManager _inputManager)
    {
        attackManager = attackManger;
        inputManager = _inputManager;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
}