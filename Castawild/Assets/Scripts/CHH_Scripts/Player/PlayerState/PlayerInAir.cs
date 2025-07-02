public class PlayerInAir : PlayerState
{
    public PlayerInAir(Player _player, PlayerStateMachine _stateMachine, string _animName)
        : base(_player, _stateMachine, _animName)
    {
    }

    public override void EnterState()
    {
        player.anim.SetBool(animName, true);
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        player.anim.SetBool(animName, false);
        player.anim.SetBool("isJumping", false);
    }
}