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
            && toolStateManager.HoldAimTool())
            toolStateManager.ChangeState(toolStateManager.aimState);

        // UseTool
        else if (toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.toolUseInput))
        {
            toolStateManager.movementManager.ChangeState(toolStateManager.movementManager.idleState);
            toolStateManager.ChangeState(toolStateManager.useToolState);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}