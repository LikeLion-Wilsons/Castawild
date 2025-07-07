public class AttackIdleState : AttackBaseState
{
    public AttackIdleState(AttackStateManager _attackManager, PlayerInputManager _inputManager)
        : base(_attackManager, _inputManager)
    {
    }

    public override void EnterState()
    {
    }

    public override void UpdateState()
    {
        if (inputManager.aimAction.WasPressedThisFrame())
            attackManager.ChangeState(attackManager.aimState);
        else if (inputManager.attackAction.WasPressedThisFrame())
            attackManager.ChangeState(attackManager.attackState);
    }

    public override void ExitState()
    {
    }
}