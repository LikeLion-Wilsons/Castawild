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
            if (toolStateManager.player.currentToolType == ToolType.Sword
                && toolStateManager.movementManager.currentState == toolStateManager.movementManager.jumpState)
                return;
            toolStateManager.ChangeState(toolStateManager.useToolState);
        }
    }

    public override void ExitState()
    {
    }
}