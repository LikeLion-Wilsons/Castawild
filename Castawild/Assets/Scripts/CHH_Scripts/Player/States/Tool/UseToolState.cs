
using UnityEngine;

public class UseToolState : ToolBaseState
{
    private int comboCount = 1;
    private float elapsed = 0f;
    private float rotateTime = 0.2f;
    public UseToolState(ToolStateManager _toolStateManager, PlayerInputManager _inputManager)
        : base(_toolStateManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        toolStateManager.CurrentToolAnimationState = ToolAnimationState.UseTool;
        toolStateManager.movementManager.Host_ChangeState(MovementState.Idle);
        toolStateManager.playerController.Host_FreezePosition(true);

        toolStateManager.DecreaseToolDuration = false;
        toolStateManager.IsDecreased = false;

        if (toolStateManager.CurrentToolType == ToolType.Fist || toolStateManager.CurrentToolType == ToolType.Throw)
        {
            toolStateManager.Client_ArmVisibleChanged(true);
        }

        else if (toolStateManager.CurrentToolType == ToolType.Bow)
        {
            toolStateManager.player.All_SetBowPos(true);
            toolStateManager.All_SetArrowPull(true);
        }

        elapsed = 0f;
    }

    public override void UpdateState()
    {
        if (elapsed <= rotateTime)
        {
            elapsed += toolStateManager.Runner.DeltaTime;
            if (toolStateManager.input.currentView == ViewType.ThirdPerson)
                toolStateManager.playerController.All_RotateForward(toolStateManager.input);
        }

        if (toolStateManager.HasStateAuthority && toolStateManager.DecreaseToolDuration && !toolStateManager.IsDecreased
            && toolStateManager.All_IsDecreaseDurationTool())
        {
            toolStateManager.IsDecreased = true;
            toolStateManager.player.inventory.RPC_SubtractDurability(toolStateManager.player.currentToolInfoData.durability);
        }

        if (toolStateManager.input.IsUp(PlayerNetworkInputData.aimInput))
            toolStateManager.Client_SetAimCameraAndUI(false);

        // 곡괭이, 도끼는 손 때까지 상태 유지
        if (CraftingToolActionRelease())
            return;

        // 콤보 어택
        if (toolStateManager.input.IsDown(PlayerNetworkInputData.toolUseInput))
        {
            if (CanComboAttack() && comboCount == 1)
            {
                toolStateManager.IsDecreased = false;
                toolStateManager.DecreaseToolDuration = false;
                comboCount++;
                toolStateManager.CanComboAttack = true;
                return;
            }
        }

        if (toolStateManager.IsAnimationFinished)
        {
            if (toolStateManager.input.IsDown(PlayerNetworkInputData.aimInput) && toolStateManager.All_HoldAimTool())
                toolStateManager.Host_ChangeState(ToolState.Aim);
            else
            {
                toolStateManager.Host_ChangeState(ToolState.Idle);
                if (toolStateManager.HasInputAuthority)
                    toolStateManager.Client_SetAimCameraAndUI(false);
            }
        }
    }

    public override void ExitState()
    {
        base.ExitState();

        toolStateManager.IsDecreased = false;
        toolStateManager.DecreaseToolDuration = false;

        if (toolStateManager.CurrentToolType == ToolType.Bow && toolStateManager.input.IsUp(PlayerNetworkInputData.aimInput))
            toolStateManager.player.All_SetBowPos(false);

        if (toolStateManager.CurrentToolType == ToolType.Throw)
            toolStateManager.player.All_SetPebbleActive(true);

        if (toolStateManager.CurrentToolType == ToolType.Fist)
            toolStateManager.Client_ArmVisibleChanged(false);

        toolStateManager.playerController.Host_FreezePosition(false);

        comboCount = 1;
        toolStateManager.ComboAttack = false;
    }

    private bool CraftingToolActionRelease()
    {
        if (toolStateManager.All_HoldCraftingTool())
        {
            if (!toolStateManager.input.IsDown(PlayerNetworkInputData.toolUseInput) && toolStateManager.IsAnimationFinished)
                toolStateManager.Host_ChangeState(ToolState.Idle);
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
}