public class AttackState : AttackBaseState
{
    public AttackState(AttackStateManager _attackManager, PlayerInputManager _inputManager)
        : base(_attackManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        if (attackManager.movementManager.currentState == attackManager.movementManager.idleState)
            attackManager.anim.SetBool("FullAttack", true);
        attackManager.anim.SetBool("Attack", true);
    }

    public override void UpdateState()
    {
        if (inputManager.moveInput.magnitude != 0f)
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
    }
}