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
        if (toolStateManager.HasInputAuthority)
            toolStateManager.movementManager.RPC_RequestCanMove(false);
        toolStateManager.movementManager.ChangeState(toolStateManager.movementManager.idleState);

        toolStateManager.CurrentToolUseState = ToolAnimationState.FullUse;
        SetActiveArmMesh(true);
    }

    public override void UpdateState()
    {
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
                toolStateManager.ChangeState(toolStateManager.idleState);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
        SetActiveArmMesh(false);

        if (toolStateManager.HasInputAuthority)
            toolStateManager.movementManager.RPC_RequestCanMove(true);
        toolStateManager.player.isAimLocked = false;

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
        if (toolStateManager.CurrentToolType == ToolType.Fist && toolStateManager.cameraManager.currentView == ViewType.FirstPerson
            && toolStateManager.HasInputAuthority)
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