
public class UseToolState : ToolBaseState
{
    private int comboCount = 1;
    private float elapsed = 0f;
    private float rotateTime = 0.2f;

    public UseToolState(ToolStateManager _toolStateManager)
        : base(_toolStateManager)
    {
    }

    public override void EnterState()
    {
        toolStateManager.CurrentToolAnimationState = ToolAnimationState.UseTool;
        toolStateManager.movementManager.Host_ChangeState(MovementState.Idle);
        toolStateManager.moveManager.Host_FreezePosition(true);

        toolStateManager.DecreaseToolDuration = false;

        if (toolStateManager.CurrentToolType == ToolType.Fist || toolStateManager.CurrentToolType == ToolType.Throw)
        {
            toolStateManager.Client_ArmVisibleChanged(true);
        }

        else if (toolStateManager.CurrentToolType == ToolType.Bow)
        {
            toolStateManager.toolManager.RPC_NotifySetBowPos(true);
            toolStateManager.toolManager.All_SetArrowActive(true);
            toolStateManager.RPC_NotifySetArrowPull(true);
        }

        elapsed = 0f;
    }

    public override void UpdateState()
    {
        if (elapsed <= rotateTime)
        {
            elapsed += toolStateManager.Runner.DeltaTime;
            if (toolStateManager.input.currentView == ViewType.ThirdPerson)
                toolStateManager.moveManager.All_RotateForward(toolStateManager.input);
        }

        if (toolStateManager.input.IsUp(PlayerNetworkInputData.aimInput))
        {
            toolStateManager.flagManager.Clear(PlayerFlags.Aim);
            toolStateManager.RPC_ApplySetAimCameraAndUI(false);

            if (toolStateManager.CurrentToolType == ToolType.Bow)
                toolStateManager.RPC_NotifySetArrowPull(false);
        }

        // 곡괭이, 도끼는 손 때까지 상태 유지
        if (CraftingToolActionRelease())
            return;

        // 콤보 어택
        if (toolStateManager.input.IsDown(PlayerNetworkInputData.toolUseInput))
        {
            if (CanComboAttack() && comboCount == 1)
            {
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
                    toolStateManager.RPC_ApplySetAimCameraAndUI(false);
            }
        }
    }

    public override void ExitState()
    {
        base.ExitState();

        if (toolStateManager.HasStateAuthority && toolStateManager.DecreaseToolDuration
            && toolStateManager.All_IsDecreaseDurationTool())
            toolStateManager.player.inventory.RPC_SubtractDurability(toolStateManager.toolManager.currentToolInfoData.durability);

        toolStateManager.DecreaseToolDuration = false;

        if (toolStateManager.CurrentToolType == ToolType.Bow && toolStateManager.input.IsUp(PlayerNetworkInputData.aimInput))
            toolStateManager.toolManager.RPC_NotifySetBowPos(false);

        if (toolStateManager.CurrentToolType == ToolType.Throw)
            toolStateManager.toolManager.All_SetPebbleActive(true);

        if (toolStateManager.CurrentToolType == ToolType.Fist)
            toolStateManager.Client_ArmVisibleChanged(false);

        toolStateManager.moveManager.Host_FreezePosition(false);

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