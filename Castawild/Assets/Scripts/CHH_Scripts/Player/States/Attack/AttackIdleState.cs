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
        // Aim
        if (inputManager.aimAction.WasPressedThisFrame() 
            && attackManager.player.currentWeaponType != WeaponType.None && attackManager.player.currentWeaponType != WeaponType.Fist)
            attackManager.ChangeState(attackManager.aimState);

        // Attack
        else if (inputManager.attackAction.WasPressedThisFrame())
            attackManager.ChangeState(attackManager.attackState);
    }

    public override void ExitState()
    {
    }
}