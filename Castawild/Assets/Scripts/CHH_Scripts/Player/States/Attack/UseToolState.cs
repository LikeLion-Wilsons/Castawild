
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
        if (toolStateManager.networkManager.HoldAttackTool())
        {
            toolStateManager.networkManager.CanMove = false;
            toolStateManager.movementManager.ChangeState(toolStateManager.movementManager.idleState);
        }

        if (toolStateManager.movementManager.currentState == toolStateManager.movementManager.idleState)
        {
            Debug.Log("EnterState FullUse");
            toolStateManager.networkManager.CurrentToolUseState = ToolAnimationState.FullUse;
        }

        else if (toolStateManager.movementManager.currentState != toolStateManager.movementManager.idleState)
            toolStateManager.networkManager.CurrentToolUseState = ToolAnimationState.Use;

        toolStateManager.player.currentAttackType = AttackType.Attack;
    }

    public override void UpdateState()
    {
        // 움직이면 상체 레이어만 적용
        if (toolStateManager.player.currentMoveType != MoveType.Idle)
            toolStateManager.networkManager.CurrentToolUseState = ToolAnimationState.Use;
        else if (toolStateManager.player.currentMoveType == MoveType.Idle)
        {
            Debug.Log("UpdateState FullUse");
            toolStateManager.networkManager.CurrentToolUseState = ToolAnimationState.FullUse;
        }

        // 곡괭이, 도구는 손 때까지 상태 유지
        if (CraftingToolActionRelease())
            return;

        // 콤보 어택
        if (toolStateManager.input.IsDown(PlayerNetworkInputData.toolUseInput))
        {
            if (ComboAttack() && comboCount == 1)
            {
                comboCount++;
                toolStateManager.animTrigger.canComboAttack = true;
                return;
            }
        }

        if (toolStateManager.networkManager.IsAnimationFinished)
        {
            if (toolStateManager.input.IsDown(PlayerNetworkInputData.aimInput) && toolStateManager.player.HoldAimTool())
                toolStateManager.ChangeState(toolStateManager.aimState);
            else
                toolStateManager.ChangeState(toolStateManager.idleState);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
        toolStateManager.networkManager.CanMove = true;
        toolStateManager.player.isAimLocked = false;

        comboCount = 1;
        toolStateManager.comboAttack = false;

        toolStateManager.player.currentAttackType = AttackType.None;
    }

    private bool CraftingToolActionRelease()
    {
        if (toolStateManager.player.HoldCraftingTool())
        {
            if (!toolStateManager.input.IsDown(PlayerNetworkInputData.toolUseInput) && toolStateManager.networkManager.IsAnimationFinished)
                toolStateManager.ChangeState(toolStateManager.idleState);
            return true;
        }
        return false;
    }

    private bool ComboAttack()
    {
        ToolType type = toolStateManager.networkManager.CurrentToolType;

        bool isMelee = type == ToolType.Sword || type == ToolType.Fist;
        bool pressed = toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.toolUseInput);
        bool canCombo = toolStateManager.networkManager.CanReceiveInput;

        return isMelee && pressed && canCombo;
    }
}