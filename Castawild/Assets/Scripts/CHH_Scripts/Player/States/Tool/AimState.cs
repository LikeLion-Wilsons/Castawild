using UnityEngine;

public class AimState : ToolBaseState
{
    public AimState(ToolStateManager _toolStateManager)
        : base(_toolStateManager)
    {
    }

    public override void EnterState()
    {
        if (toolStateManager.flagManager.IsMoveIdle)
            toolStateManager.CurrentToolAnimationState = ToolAnimationState.FullAim;
        else
            toolStateManager.CurrentToolAnimationState = ToolAnimationState.Aim;

        LookForward();

        if (toolStateManager.flagManager.IsRunning)
            toolStateManager.movementManager.Host_ChangeState(MovementState.Walk);

        if (toolStateManager.CurrentToolType == ToolType.Bow)
        {
            toolStateManager.toolManager.All_SetArrowActive(true);
            if (toolStateManager.HasStateAuthority)
            {
                toolStateManager.toolManager.RPC_NotifySetBowPos(true);
                toolStateManager.RPC_NotifySetArrowPull(true);
            }
        }

        toolStateManager.flagManager.Set(PlayerFlags.Aim);
        if (toolStateManager.HasInputAuthority)
            toolStateManager.Client_SetAimCameraAndUI(true);
    }

    public override void UpdateState()
    {
        if (toolStateManager.input.currentView == ViewType.ThirdPerson)
            toolStateManager.Host_RotatePlayer(true);

        if (toolStateManager.flagManager.IsMoveIdle)
            toolStateManager.CurrentToolAnimationState = ToolAnimationState.FullAim;
        else
            toolStateManager.CurrentToolAnimationState = ToolAnimationState.Aim;

        if (toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.toolUseInput) && toolStateManager.player.Stamina >= 10f)
        {
            if (toolStateManager.CurrentToolType == ToolType.Bow)
                toolStateManager.RPC_NotifyBowShootAnimation();

            toolStateManager.Host_ChangeState(ToolState.UseTool);
        }

        else if (toolStateManager.input.IsUp(PlayerNetworkInputData.aimInput))
        {
            toolStateManager.flagManager.Clear(PlayerFlags.Aim);

            if (toolStateManager.CurrentToolType == ToolType.Bow)
            {
                if (toolStateManager.HasStateAuthority)
                {
                    toolStateManager.toolManager.RPC_NotifySetBowPos(false);
                    toolStateManager.RPC_NotifySetArrowPull(false);
                }
            }
            toolStateManager.RPC_ApplySetAimCameraAndUI(false);
            toolStateManager.Host_ChangeState(ToolState.Idle);
        }
    }

    public override void ExitState()
    {
        base.ExitState();

        if (toolStateManager.input.currentView == ViewType.ThirdPerson)
            toolStateManager.Host_RotatePlayer(false);
    }

    private void LookForward()
    {
        Vector3 lookDir = toolStateManager.cameraManager.CurrenCam.transform.forward;
        lookDir.y = 0f;
        toolStateManager.transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
