
using UnityEngine;

public abstract class ToolBaseState : BaseState
{
    protected ToolStateManager toolStateManager;

    public ToolBaseState(ToolStateManager _toolStateManager, PlayerInputManager _inputManager)
    {
        toolStateManager = _toolStateManager;
        inputManager = _inputManager;
    }
}