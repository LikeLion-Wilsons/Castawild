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

        if (ToolActionRelease())
            return;

        if (toolStateManager.animTrigger.isAnimationFinished)
        {
            toolStateManager.animTrigger.isAnimationFinished = false;

            if (inputManager.aimAction.IsPressed() && toolStateManager.player.IsAimTool())
                toolStateManager.ChangeState(toolStateManager.aimState);
            else
                toolStateManager.ChangeState(toolStateManager.idleState);
        }
    }

    public override void ExitState()
    {
        toolStateManager.player.isAimLocked = false;

        toolStateManager.anim.SetBool("FullUseTool", false);
        toolStateManager.anim.SetBool("UseTool", false);
        toolStateManager.player.currentAttackType = AttackType.None;
    }

    private bool ToolActionRelease()
    {
        if (toolStateManager.player.IsTool())
        {
            if (!inputManager.toolAction.IsPressed() && toolStateManager.animTrigger.isAnimationFinished)
                toolStateManager.ChangeState(toolStateManager.idleState);
            return true;
        }
        return false;
    }
}