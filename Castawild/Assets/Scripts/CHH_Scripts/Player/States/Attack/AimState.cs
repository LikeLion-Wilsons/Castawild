using UnityEngine;

public class AimState : AttackBaseState
{
    public AimState(AttackStateManager _attackManager, PlayerInputManager _inputManager)
        : base(_attackManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        attackManager.cameraManager.LockCameraInput();

        if (attackManager.movementManager.currentState == attackManager.movementManager.idleState)
            attackManager.anim.SetBool("FullAiming", true);
        attackManager.anim.SetBool("Aiming", true);
    }

    public override void UpdateState()
    {
        attackManager.xAxis += inputManager.lookInput.y * attackManager.mouseSense;
        attackManager.yAxis += inputManager.lookInput.x * attackManager.mouseSense;
        attackManager.yAxis = Mathf.Clamp(attackManager.yAxis, -80f, 80f);

        if (inputManager.attackAction.WasReleasedThisFrame())
            attackManager.ChangeState(attackManager.attackState);
        else if (inputManager.aimAction.WasReleasedThisFrame())
            attackManager.ChangeState(attackManager.idleState);
    }

    public override void ExitState()
    {
        attackManager.cameraManager.UnlockCameraInput();
        attackManager.anim.SetBool("FullAiming", false);
        attackManager.anim.SetBool("Aiming", false);
    }
}
