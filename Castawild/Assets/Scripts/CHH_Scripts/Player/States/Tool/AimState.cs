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
            toolStateManager.CurrentToolUseState = ToolAnimationState.FullAim;
        else
            toolStateManager.CurrentToolUseState = ToolAnimationState.Aim;

        if (toolStateManager.movementManager.currentState == toolStateManager.movementManager.runState)
            toolStateManager.movementManager.ChangeState(toolStateManager.movementManager.walkState);

        if (toolStateManager.HasStateAuthority)
            toolStateManager.RPC_MoveAimCamera(true);

        if (toolStateManager.CurrentToolType == ToolType.Bow)
            toolStateManager.RPC_BowSetting(true);

        toolStateManager.player.playerInteractUI.crosshairImage.gameObject.SetActive(true);
    }

    public override void UpdateState()
    {
        RotatePlayer();

        if (toolStateManager.movementManager.currentState == toolStateManager.movementManager.idleState)
            toolStateManager.CurrentToolUseState = ToolAnimationState.FullAim;
        else
            toolStateManager.CurrentToolUseState = ToolAnimationState.Aim;

        if (toolStateManager.input.WasPressed(toolStateManager.prevInputButtons, PlayerNetworkInputData.toolUseInput))
        {
            if (toolStateManager.CurrentToolType == ToolType.Bow)
                toolStateManager.RPC_BowShoot();

            toolStateManager.ChangeState(toolStateManager.useToolState);
        }

        else if (toolStateManager.input.IsUp(PlayerNetworkInputData.aimInput))
        {
            toolStateManager.player.isAimLocked = false;

            if (toolStateManager.HasStateAuthority)
                toolStateManager.RPC_MoveAimCamera(false);

            if (toolStateManager.CurrentToolType == ToolType.Bow)
                toolStateManager.RPC_BowSetting(false);

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
    }

    private void LookForward()
    {
        Vector3 lookDir = toolStateManager.cameraManager.CurrenCam.transform.forward;
        lookDir.y = 0f;
        toolStateManager.transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
