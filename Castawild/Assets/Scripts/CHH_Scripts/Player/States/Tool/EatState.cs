using Unity.VisualScripting;
using UnityEngine;

public class EatState : ToolBaseState
{
    public EatState(ToolStateManager _toolStateManager, PlayerInputManager _inputManager)
        : base(_toolStateManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        if (toolStateManager.player.currentItemType == ItemType.Food)
            toolStateManager.CurrentToolAnimationState = ToolAnimationState.Eat;
        if (toolStateManager.player.currentItemType == ItemType.Drink)
            toolStateManager.CurrentToolAnimationState = ToolAnimationState.Drink;

        toolStateManager.playerController.Host_FreezePosition(true);
    }

    public override void UpdateState()
    {
        if (toolStateManager.IsAnimationFinished)
            toolStateManager.Host_ChangeState(ToolState.Idle);
    }

    public override void ExitState()
    {
        base.ExitState();
        if (toolStateManager.HasStateAuthority)
            toolStateManager.player.Host_RestoreStatFromFood();

        toolStateManager.playerController.Host_FreezePosition(false);
    }
}
