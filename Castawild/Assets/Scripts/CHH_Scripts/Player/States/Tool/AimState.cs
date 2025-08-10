using UnityEngine;

public class AimState : ToolBaseState
{
    public AimState(ToolStateManager _toolStateManager, PlayerInputManager _inputManager)
        : base(_toolStateManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        // 애니메이션
        if (toolStateManager.movementManager.CurrentMoveState == MovementState.Idle)
            toolStateManager.CurrentToolAnimationState = ToolAnimationState.FullAim;
        else
            toolStateManager.CurrentToolAnimationState = ToolAnimationState.Aim;

        LookForward();

        // Movement상태
        if (toolStateManager.movementManager.CurrentMoveState == MovementState.Run)
            toolStateManager.movementManager.Host_ChangeState(MovementState.Walk);

        toolStateManager.Client_SetAimCameraAndUI(true);

        if (toolStateManager.CurrentToolType == ToolType.Bow)
        {
            toolStateManager.player.All_SetBowPos(true);
            toolStateManager.player.All_SetArrowActive(true);
            toolStateManager.All_SetArrowPull(true);
        }
    }

    public override void UpdateState()
    {
        if (toolStateManager.input.currentView == ViewType.ThirdPerson)
            toolStateManager.All_RotatePlayer();

        if (toolStateManager.movementManager.CurrentMoveState == MovementState.Idle)
            toolStateManager.CurrentToolAnimationState = ToolAnimationState.FullAim;
        else
            toolStateManager.CurrentToolAnimationState = ToolAnimationState.Aim;

        if (toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.toolUseInput))
        {
            if (toolStateManager.CurrentToolType == ToolType.Bow)
                toolStateManager.All_BowShootAnimation();

            toolStateManager.Host_ChangeState(ToolState.UseTool);
        }

        else if (toolStateManager.input.IsUp(PlayerNetworkInputData.aimInput))
        {
            if (toolStateManager.CurrentToolType == ToolType.Bow)
            {
                toolStateManager.player.All_SetBowPos(false);
                toolStateManager.All_SetArrowPull(false);
            }
            toolStateManager.Client_SetAimCameraAndUI(false);
            toolStateManager.Host_ChangeState(ToolState.Idle);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
    }



    private void LookForward()
    {
        Vector3 lookDir = toolStateManager.cameraManager.CurrenCam.transform.forward;
        lookDir.y = 0f;
        toolStateManager.transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
