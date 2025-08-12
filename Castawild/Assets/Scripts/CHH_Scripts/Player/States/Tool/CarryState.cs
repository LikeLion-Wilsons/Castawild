using UnityEngine;

public class CarryState : ToolBaseState
{
    public CarryState(ToolStateManager _toolStateManager)
        : base(_toolStateManager)
    {
    }

    public override void EnterState()
    {
        toolStateManager.CurrentToolAnimationState = ToolAnimationState.Carry;
        toolStateManager.flagManager.Set(PlayerFlags.Carry);
    }

    public override void UpdateState()
    {
        if (toolStateManager.input.currentView == ViewType.ThirdPerson)
            toolStateManager.Host_RotatePlayer(true);

        if (toolStateManager.player.currentItemType != ItemType.Placeable)
            toolStateManager.Host_ChangeState(ToolState.Idle);
    }

    public override void ExitState()
    {
        base.ExitState();
        toolStateManager.flagManager.Clear(PlayerFlags.Carry);
    }
}
