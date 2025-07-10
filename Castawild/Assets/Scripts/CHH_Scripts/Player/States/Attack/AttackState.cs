public class AttackState : AttackBaseState
{
    public AttackState(AttackStateManager _attackManager, PlayerInputManager _inputManager)
        : base(_attackManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        LookForward();

        if (attackManager.movementManager.currentState == attackManager.movementManager.idleState)
            attackManager.anim.SetBool("FullAttack", true);
        attackManager.anim.SetBool("Attack", true);

        attackManager.player.Attack();
        attackManager.player.currentAttackType = AttackType.Attack;
    }

    public override void UpdateState()
    {
        if (attackManager.player.currentMoveType != MoveType.Idle)
            attackManager.anim.SetBool("FullAttack", false);

        if (attackManager.animTrigger.isAnimationFinished)
        {
            attackManager.animTrigger.isAnimationFinished = false;
            attackManager.ChangeState(attackManager.idleState);
        }
    }

    public override void ExitState()
    {
        attackManager.anim.SetBool("FullAttack", false);
        attackManager.anim.SetBool("Attack", false);
        attackManager.player.currentAttackType = AttackType.None;
    }
}