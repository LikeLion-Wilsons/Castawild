using System.Diagnostics;

public class ToolIdleState : ToolBaseState
{
    public ToolIdleState(ToolStateManager _toolStateManager)
        : base(_toolStateManager)
    {
    }

    public override void EnterState()
    {
        toolStateManager.CurrentToolAnimationState = ToolAnimationState.Idle;
    }

    public override void UpdateState()
    {
        if (toolStateManager.player.isDead)
            return;

        // Aim
        if (toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.aimInput)
                && toolStateManager.All_HoldAimTool())
            toolStateManager.Host_ChangeState(ToolState.Aim);

        // UseTool
        else if (toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.toolUseInput))
        {
            // 음식 들고있을 땐 먹기
            if (toolStateManager.player.currentItemType == ItemType.Food || toolStateManager.player.currentItemType == ItemType.Drink)
            {
                toolStateManager.Host_ChangeState(ToolState.Eat);
                return;
            }

            toolStateManager.movementManager.Host_ChangeState(MovementState.Idle);
            toolStateManager.Host_ChangeState(ToolState.UseTool);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}