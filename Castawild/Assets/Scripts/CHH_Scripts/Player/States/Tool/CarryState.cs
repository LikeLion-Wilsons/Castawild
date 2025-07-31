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
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}
