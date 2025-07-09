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
            && toolStateManager.player.currentToolType != ToolType.None && toolStateManager.player.currentToolType != ToolType.Fist)
            toolStateManager.ChangeState(toolStateManager.aimState);

        // UseTool
        else if (inputManager.toolAction.WasPressedThisFrame())
            toolStateManager.ChangeState(toolStateManager.useToolState);
    }

    public override void ExitState()
    {
    }
}