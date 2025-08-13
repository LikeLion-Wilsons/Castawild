

public abstract class ToolBaseState : BaseState
{
    protected ToolStateManager toolStateManager;

    public ToolBaseState(ToolStateManager _toolStateManager)
    {
        toolStateManager = _toolStateManager;
    }

    public override void ExitState()
    {
        toolStateManager.IsAnimationFinished = false;
    }
}