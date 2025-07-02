public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(CwPlayer _player, PlayerStateMachine _stateMachine, string _animName)
        : base(_player, _stateMachine, _animName)
    {
    }

    public override void EnterState()
    {
        player.anim.SetBool(animName, true);
    }

    public override void UpdateState()
    {
        if (player.inputController.moveInput.magnitude == 0f)
            player.ChangeStateMachine(player.idleState);
    }

    public override void ExitState()
    {
        player.anim.SetBool(animName, false);
    }
}