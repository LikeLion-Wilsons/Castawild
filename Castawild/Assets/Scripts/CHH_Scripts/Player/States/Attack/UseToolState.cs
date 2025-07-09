public class UseToolState : ToolBaseState
{
    public UseToolState(ToolStateManager _toolStateManager, PlayerInputManager _inputManager)
        : base(_toolStateManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        if (toolStateManager.movementManager.currentState == toolStateManager.movementManager.idleState)
            toolStateManager.anim.SetBool("FullUseTool", true);
        toolStateManager.anim.SetBool("UseTool", true);

        toolStateManager.player.Attack();
        toolStateManager.player.currentAttackType = AttackType.Attack;
    }

    public override void UpdateState()
    {
        if (toolStateManager.player.currentMoveType != MoveType.Idle)
            toolStateManager.anim.SetBool("FullUseTool", false);

        if (toolStateManager.animTrigger.isAnimationFinished)
        {
            toolStateManager.animTrigger.isAnimationFinished = false;
            toolStateManager.ChangeState(toolStateManager.idleState);
        }
    }

    public override void ExitState()
    {
        toolStateManager.anim.SetBool("FullUseTool", false);
        toolStateManager.anim.SetBool("UseTool", false);
        toolStateManager.player.currentAttackType = AttackType.None;
    }
}