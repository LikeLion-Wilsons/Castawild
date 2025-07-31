public class ToolIdleState : ToolBaseState
{
    public ToolIdleState(ToolStateManager _toolStateManager, PlayerInputManager _inputManager)
        : base(_toolStateManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        toolStateManager.CurrentToolUseState = ToolAnimationState.Idle;
    }

    public override void UpdateState()
    {
        // Aim
        if (toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.aimInput)
            && toolStateManager.HoldAimTool() && toolStateManager.player.CanUseTool())
            toolStateManager.ChangeState(toolStateManager.aimState);

        // UseTool
        else if (toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.toolUseInput)
            && toolStateManager.player.CanUseTool())
        {
            toolStateManager.movementManager.ChangeState(toolStateManager.movementManager.idleState);

            // 음식 들고있을 땐 먹기 - 나중에 제대로 고치기
            if (toolStateManager.player.currentItemIdx > 999)
            {
                toolStateManager.ChangeState(toolStateManager.eatState);
                return;
            }

            toolStateManager.ChangeState(toolStateManager.useToolState);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}