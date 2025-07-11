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
            toolStateManager.anim.SetBool("FullAiming", true);
        toolStateManager.anim.SetBool("Aiming", true);

        toolStateManager.cameraManager.MoveCamera(true);

        toolStateManager.anim.SetInteger("WeaponType", (int)toolStateManager.player.currentToolType);
        toolStateManager.player.currentAttackType = AttackType.Aim;
    }

    public override void UpdateState()
    {
        RotatePlayer();

        if (toolStateManager.player.currentMoveType != MoveType.Idle)
            toolStateManager.anim.SetBool("FullAiming", false);

        if (inputManager.toolAction.WasReleasedThisFrame())
            toolStateManager.ChangeState(toolStateManager.useToolState);

        else if (inputManager.aimAction.WasReleasedThisFrame())
        {
            toolStateManager.player.isAimLocked = false;
            toolStateManager.ChangeState(toolStateManager.idleState);
        }
    }

    private void RotatePlayer()
    {
        Vector3 lookDirection = toolStateManager.cam.transform.forward;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            toolStateManager.transform.rotation = Quaternion.Slerp(toolStateManager.transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    public override void ExitState()
    {
        toolStateManager.player.currentAttackType = AttackType.None;
    }

    private void LookForward()
    {
        Vector3 lookDir = toolStateManager.cam.transform.forward;
        lookDir.y = 0f;
        toolStateManager.transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
