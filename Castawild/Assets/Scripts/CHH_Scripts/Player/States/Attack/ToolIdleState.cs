public class ToolIdleState : ToolBaseState
{
    public ToolIdleState(ToolStateManager _toolStateManager, PlayerInputManager _inputManager)
        : base(_toolStateManager, _inputManager)
    {
    }

    public override void EnterState()
    {
    }

    public override void UpdateState()
    {
        // Aim
        if (inputManager.aimAction.WasPressedThisFrame()
            && toolStateManager.player.HoldAimTool())
            toolStateManager.ChangeState(toolStateManager.aimState);

        // UseTool
        else if (inputManager.toolAction.WasPressedThisFrame())
        {
            // 점프상태일 땐 막기
            if (toolStateManager.player.HoldAttackTool())
            {
                if (toolStateManager.movementManager.currentState == toolStateManager.movementManager.jumpState)
                    return;
                toolStateManager.movementManager.ChangeIdleState();
            }
            toolStateManager.ChangeState(toolStateManager.useToolState);
        }
    }

    public override void ExitState()
    {
    }
}