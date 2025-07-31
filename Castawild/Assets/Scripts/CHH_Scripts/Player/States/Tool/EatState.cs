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
        if (toolStateManager.player.currentItemIdx > 999)
            toolStateManager.CurrentToolUseState = ToolAnimationState.Eat;
        else if (toolStateManager.player.currentItemIdx > 999)
            toolStateManager.CurrentToolUseState = ToolAnimationState.Drink;

        toolStateManager.player.CanAct = false;
    }

    public override void UpdateState()
    {
        if (toolStateManager.IsAnimationFinished)
            toolStateManager.ChangeState(toolStateManager.idleState);
    }

    public override void ExitState()
    {
        base.ExitState();
        toolStateManager.player.CanAct = true;
    }
}
