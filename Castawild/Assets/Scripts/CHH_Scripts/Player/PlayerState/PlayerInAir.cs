using UnityEngine;

public class PlayerInAir : PlayerState
{
    public PlayerInAir(CwPlayer _player, PlayerStateMachine _stateMachine, string _animName)
        : base(_player, _stateMachine, _animName)
    {
    }

    public override void EnterState()
    {
        player.anim.SetBool(animName, true);
    }

    public override void UpdateState()
    {
        if (player.isGround && player.rigid.linearVelocity.y < 0f && !player.anim.GetBool("Land"))
            player.anim.SetBool("Land", true);
    }

    public override void ExitState()
    {
        player.anim.SetBool(animName, false);
        player.canJump = true;
        player.isFalling = false;
        player.anim.SetBool("isFalling", player.isFalling);
        player.anim.SetBool("Land", false);
    }
}
