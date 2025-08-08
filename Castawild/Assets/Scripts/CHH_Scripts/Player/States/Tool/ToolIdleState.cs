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
        if (toolStateManager.movementManager.IsDeath())
            return;

        // Aim
        if (toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.aimInput)
            && toolStateManager.HoldAimTool() && toolStateManager.player.CanUseTool())
            toolStateManager.ChangeState(ToolState.Aim);

        // UseTool
        else if (toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.toolUseInput)
            && toolStateManager.player.CanUseTool())
        {
            toolStateManager.movementManager.ChangeState(MovementState.Idle);

            // 음식 들고있을 땐 먹기
            if (toolStateManager.player.currentItemType == ItemType.Food || toolStateManager.player.currentItemType == ItemType.Drink)
            {
                toolStateManager.ChangeState(ToolState.Eat);
                return;
            }

            toolStateManager.ChangeState(ToolState.UseTool);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}