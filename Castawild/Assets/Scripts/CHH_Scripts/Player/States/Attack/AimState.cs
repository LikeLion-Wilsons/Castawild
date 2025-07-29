using UnityEngine;

public class AimState : ToolBaseState
{
    public AimState(ToolStateManager _toolStateManager, PlayerInputManager _inputManager)
        : base(_toolStateManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        LookForward();

        toolStateManager.player.isAimLocked = true;

        if (toolStateManager.movementManager.currentState == toolStateManager.movementManager.idleState)
            toolStateManager.CurrentToolUseState = ToolAnimationState.Aim;
        else
            toolStateManager.CurrentToolUseState = ToolAnimationState.FullAim;

        if (toolStateManager.movementManager.currentState == toolStateManager.movementManager.runState)
            toolStateManager.movementManager.ChangeState(toolStateManager.movementManager.walkState);

        toolStateManager.cameraManager.MoveCamera(true);

        toolStateManager.player.crosshairImage.SetActive(true);
    }

    public override void UpdateState()
    {
        RotatePlayer();

        if (toolStateManager.player.currentMoveType != MoveType.Idle)
            toolStateManager.CurrentToolUseState = ToolAnimationState.Aim;
        else if (toolStateManager.player.currentMoveType == MoveType.Idle)
            toolStateManager.CurrentToolUseState = ToolAnimationState.FullAim;

        if (toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.toolUseInput))
            toolStateManager.ChangeState(toolStateManager.useToolState);

        else if (toolStateManager.input.IsUp(PlayerNetworkInputData.aimInput))
        {
            toolStateManager.player.isAimLocked = false;
            toolStateManager.cameraManager.MoveCamera(false);
            toolStateManager.ChangeState(toolStateManager.idleState);
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
        toolStateManager.player.crosshairImage.SetActive(false);
    }

    private void LookForward()
    {
        Vector3 lookDir = toolStateManager.cameraManager.CurrenCam.transform.forward;
        lookDir.y = 0f;
        toolStateManager.transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
