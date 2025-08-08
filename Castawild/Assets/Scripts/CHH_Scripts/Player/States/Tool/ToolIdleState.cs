using System.Diagnostics;

public class ToolIdleState : ToolBaseState
{
    public ToolIdleState(ToolStateManager _toolStateManager, PlayerInputManager _inputManager)
        : base(_toolStateManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        toolStateManager.CurrentToolAnimationState = ToolAnimationState.Idle;
    }

    public override void UpdateState()
    {
        if (toolStateManager.player.IsDeath())
            return;

        // Aim
        if (toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.aimInput)
            && toolStateManager.All_HoldAimTool() && toolStateManager.player.All_CanUseTool())
            toolStateManager.Host_ChangeState(ToolState.Aim);

        // UseTool
        else if (toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.toolUseInput)
            && toolStateManager.player.All_CanUseTool())
        {
            toolStateManager.movementManager.Host_ChangeState(MovementState.Idle);

            // 음식 들고있을 땐 먹기
            if (toolStateManager.player.currentItemType == ItemType.Food || toolStateManager.player.currentItemType == ItemType.Drink)
            {
                toolStateManager.Host_ChangeState(ToolState.Eat);
                return;
            }

            toolStateManager.Host_ChangeState(ToolState.UseTool);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}