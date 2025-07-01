public class PlayerAttackState : PlayerState
{
    public PlayerAttackState(Player player, PlayerStateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName)
    {
    }

    public override void EnterState()
    {
        player.currentWeapon.Attack();
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
    }
}