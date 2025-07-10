public class UseToolState : ToolBaseState
{
    private int comboCount = 1;

    public UseToolState(ToolStateManager _toolStateManager, PlayerInputManager _inputManager)
        : base(_toolStateManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        toolStateManager.animTrigger.canComboAttack = true;

        if (toolStateManager.movementManager.currentState == toolStateManager.movementManager.idleState)
            toolStateManager.anim.SetBool("FullUseTool", true);
        toolStateManager.anim.SetBool("UseTool", true);

        toolStateManager.player.Attack();
        toolStateManager.player.currentAttackType = AttackType.Attack;
    }

    public override void UpdateState()
    {
        // 움직이면 상체 레이어만 적용
        if (toolStateManager.player.currentMoveType != MoveType.Idle)
            toolStateManager.anim.SetBool("FullUseTool", false);

        // 곡괭이, 도구는 손 때까지 상태 유지
        if (CraftingToolActionRelease())
            return;

        if (inputManager.toolAction.WasPressedThisFrame())
        // 콤보 어택
        {
            if (ComboAttack() && comboCount == 1)
            {
                comboCount++;
                toolStateManager.anim.SetBool("ComboAttack", true);
                return;
            }
        }

        if (toolStateManager.animTrigger.isAnimationFinished)
        {
            toolStateManager.animTrigger.isAnimationFinished = false;

            if (inputManager.aimAction.IsPressed() && toolStateManager.player.HoldAimTool())
                toolStateManager.ChangeState(toolStateManager.aimState);
            else
                toolStateManager.ChangeState(toolStateManager.idleState);
        }
    }

    public override void ExitState()
    {
        comboCount = 1;
        toolStateManager.animTrigger.canComboAttack = true;

        toolStateManager.player.isAimLocked = false;

        toolStateManager.anim.SetBool("FullUseTool", false);
        toolStateManager.anim.SetBool("UseTool", false);
        toolStateManager.anim.SetBool("ComboAttack", false);

        toolStateManager.player.currentAttackType = AttackType.None;
    }

    private bool CraftingToolActionRelease()
    {
        if (toolStateManager.player.HoldCraftingTool())
        {
            if (!inputManager.toolAction.IsPressed() && toolStateManager.animTrigger.isAnimationFinished)
                toolStateManager.ChangeState(toolStateManager.idleState);
            return true;
        }
        return false;
    }

    private bool ComboAttack()
    {
        ToolType type = toolStateManager.player.currentToolType;

        bool isMelee = type == ToolType.Sword || type == ToolType.Fist;
        bool pressed = inputManager.toolAction.WasPressedThisFrame();
        bool canCombo = toolStateManager.animTrigger.canComboAttack;

        return isMelee && pressed && canCombo;
    }
}