using UnityEngine;

public class CarryState : ToolBaseState
{
    public CarryState(ToolStateManager _toolStateManager, PlayerInputManager _inputManager)
        : base(_toolStateManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        toolStateManager.CurrentToolUseState = ToolAnimationState.Carry;
    }

    public override void UpdateState()
    {
        if (toolStateManager.player.currentItemType != ItemType.Placeable)
            toolStateManager.ChangeState(toolStateManager.idleState);
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}
