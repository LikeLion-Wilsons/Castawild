using UnityEngine;

public class UseToolState : ToolBaseState
{
    private int comboCount = 1;

    public UseToolState(ToolStateManager _toolStateManager, PlayerInputManager _inputManager)
        : base(_toolStateManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        toolStateManager.playerController.RPC_FreezePosition(true);
        toolStateManager.movementManager.ChangeState(toolStateManager.movementManager.idleState);

        toolStateManager.CurrentToolUseState = ToolAnimationState.FullUse;

        if (toolStateManager.CurrentToolType == ToolType.Fist || toolStateManager.CurrentToolType == ToolType.Throw)
        {
            SetActiveArmMesh(true);
            if (toolStateManager.CurrentToolType == ToolType.Throw)
                toolStateManager.player.CurrentToolActive(true);
        }

        else if (toolStateManager.CurrentToolType == ToolType.Bow)
            toolStateManager.RPC_BowAim(true);
    }

    public override void UpdateState()
    {
        if (toolStateManager.input.IsUp(PlayerNetworkInputData.aimInput))
            toolStateManager.StartAim(false);

        // 곡괭이, 도끼는 손 때까지 상태 유지
        if (CraftingToolActionRelease())
            return;

        // 콤보 어택
        if (toolStateManager.input.IsDown(PlayerNetworkInputData.toolUseInput))
        {
            if (CanComboAttack() && comboCount == 1)
            {
                comboCount++;
                toolStateManager.CanComboAttack = true;
                return;
            }
        }

        if (toolStateManager.IsAnimationFinished)
        {
            if (toolStateManager.input.IsDown(PlayerNetworkInputData.aimInput) && toolStateManager.HoldAimTool())
                toolStateManager.ChangeState(toolStateManager.aimState);
            else
            {
                toolStateManager.ChangeState(toolStateManager.idleState);
            }
        }
    }

    public override void ExitState()
    {
        base.ExitState();

        if (toolStateManager.CurrentToolType == ToolType.Fist)
            SetActiveArmMesh(false);

        else if (toolStateManager.CurrentToolType == ToolType.Bow)
            toolStateManager.RPC_BowAim(false);

        toolStateManager.playerController.RPC_FreezePosition(false);

        comboCount = 1;
        toolStateManager.ComboAttack = false;
    }

    private bool CraftingToolActionRelease()
    {
        if (toolStateManager.HoldCraftingTool())
        {
            if (!toolStateManager.input.IsDown(PlayerNetworkInputData.toolUseInput) && toolStateManager.IsAnimationFinished)
                toolStateManager.ChangeState(toolStateManager.idleState);
            return true;
        }
        return false;
    }

    private bool CanComboAttack()
    {
        ToolType type = toolStateManager.CurrentToolType;

        bool isMelee = type == ToolType.Sword || type == ToolType.Fist;
        bool pressed = toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.toolUseInput);
        bool canCombo = toolStateManager.CanReceiveInput;

        return isMelee && pressed && canCombo;
    }

    public void SetActiveArmMesh(bool isActive)
    {
        toolStateManager.RPC_ArmVisibleChanged(isActive);
    }
}