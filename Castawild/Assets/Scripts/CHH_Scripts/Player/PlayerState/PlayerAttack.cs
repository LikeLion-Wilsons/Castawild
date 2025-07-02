public class PlayerAttackState : PlayerState
{
    public PlayerAttackState(Player _player, PlayerStateMachine _stateMachine, string _animName)
        : base(_player, _stateMachine, _animName)
    {
    }

    public override void EnterState()
    {
        player.anim.SetTrigger(animName);
        player.currentWeapon.Attack();
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
    }
}