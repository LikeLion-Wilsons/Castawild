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
        toolStateManager.moveManager.CanRun_Tool = false;
    }

    public override void UpdateState()
    {
        if (toolStateManager.input.currentView == ViewType.ThirdPerson)
            toolStateManager.All_RotatePlayer();

        if (toolStateManager.player.currentItemType != ItemType.Placeable)
            toolStateManager.Host_ChangeState(ToolState.Idle);
    }

    public override void ExitState()
    {
        base.ExitState();
        toolStateManager.moveManager.CanRun_Tool = true;
    }
}
