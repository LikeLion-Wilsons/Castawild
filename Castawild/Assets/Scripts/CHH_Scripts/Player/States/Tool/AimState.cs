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
            toolStateManager.movementManager.ChangeState(MovementState.Walk);

        toolStateManager.StartAim(true);
    }

    public override void UpdateState()
    {
        RotatePlayer();

        if (toolStateManager.movementManager.CurrentMoveState == MovementState.Idle)
            toolStateManager.CurrentToolAnimationState = ToolAnimationState.FullAim;
        else
            toolStateManager.CurrentToolAnimationState = ToolAnimationState.Aim;

        if (toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.toolUseInput))
        {
            if (toolStateManager.CurrentToolType == ToolType.Bow)
                toolStateManager.RPC_BowShootAnimation();

            toolStateManager.ChangeState(ToolState.UseTool);
        }

        else if (toolStateManager.input.IsUp(PlayerNetworkInputData.aimInput))
        {
            toolStateManager.StartAim(false);
            toolStateManager.ChangeState(ToolState.Idle);
        }
    }

    private void RotatePlayer()
    {
        Vector3 lookDirection = toolStateManager.cameraManager.CurrenCam.transform.forward;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            toolStateManager.transform.rotation = Quaternion.Slerp(toolStateManager.transform.rotation, targetRotation, Time.deltaTime * 10f);
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
