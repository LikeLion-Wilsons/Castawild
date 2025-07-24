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
        toolStateManager.networkManager.CanMove = false;
        toolStateManager.movementManager.ChangeState(toolStateManager.movementManager.idleState);

        toolStateManager.networkManager.CurrentToolUseState = ToolAnimationState.FullUse;
        SetActiveArmMesh(true);
    }

    public override void UpdateState()
    {
        // 움직이면 상체 레이어만 적용
        //if (toolStateManager.player.currentMoveType != MoveType.Idle)
        //    toolStateManager.networkManager.CurrentToolUseState = ToolAnimationState.Use;
        //else if (toolStateManager.player.currentMoveType == MoveType.Idle)
        //    toolStateManager.networkManager.CurrentToolUseState = ToolAnimationState.FullUse;

        // 곡괭이, 도끼는 손 때까지 상태 유지
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
            if (toolStateManager.input.IsDown(PlayerNetworkInputData.aimInput) && toolStateManager.networkManager.HoldAimTool())
                toolStateManager.ChangeState(toolStateManager.aimState);
            else
                toolStateManager.ChangeState(toolStateManager.idleState);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
        SetActiveArmMesh(false);
        toolStateManager.networkManager.CanMove = true;
        toolStateManager.player.isAimLocked = false;

        comboCount = 1;
        toolStateManager.comboAttack = false;
    }

    private bool CraftingToolActionRelease()
    {
        if (toolStateManager.networkManager.HoldCraftingTool())
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

    public void SetActiveArmMesh(bool isActive)
    {
        if (toolStateManager.networkManager.CurrentToolType == ToolType.Fist && toolStateManager.cameraManager.currentView == ViewType.FirstPerson
            && toolStateManager.networkManager.HasInputAuthority)
        {
            toolStateManager.visibleMesh.SetActive(isActive);
            if (isActive)
            {
                toolStateManager.armature.SetParent(toolStateManager.cameraManager.firstPersonCam.transform);
                toolStateManager.armature.localPosition = new Vector3(0f, -3f, 0f);
                toolStateManager.armature.localRotation = Quaternion.identity;
            }

            if (!isActive)
            {
                toolStateManager.armature.SetParent(toolStateManager.player.transform);
                toolStateManager.armature.localPosition = Vector3.zero;
                toolStateManager.armature.localRotation = Quaternion.identity;
            }
        }
    }
}