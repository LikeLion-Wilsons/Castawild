using UnityEngine;

public class EatState : ToolBaseState
{
    public EatState(ToolStateManager _toolStateManager, PlayerInputManager _inputManager)
        : base(_toolStateManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        // 나중에 아이템 인덱스 제대로 설정하기
        if (toolStateManager.player.currentItemType == ItemType.Food)
            toolStateManager.CurrentToolUseState = ToolAnimationState.Eat;
        if (toolStateManager.player.currentItemType == ItemType.Drink)
            toolStateManager.CurrentToolUseState = ToolAnimationState.Drink;

        toolStateManager.player.StopPlayer();
    }

    public override void UpdateState()
    {
        if (toolStateManager.IsAnimationFinished)
            toolStateManager.ChangeState(toolStateManager.idleState);
    }

    public override void ExitState()
    {
        base.ExitState();
        toolStateManager.player.CanMove = true;
        toolStateManager.isTriggerSet = false;
    }
}
