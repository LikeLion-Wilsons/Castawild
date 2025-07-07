using UnityEngine;

public class AimState : AttackBaseState
{
    public AimState(AttackStateManager _attackManager, PlayerInputManager _inputManager)
        : base(_attackManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        attackManager.cameraManager.MoveCamera(true);

        if (attackManager.movementManager.currentState == attackManager.movementManager.idleState)
            attackManager.anim.SetBool("FullAiming", true);
        attackManager.anim.SetBool("Aiming", true);
    }

    public override void UpdateState()
    {
        Vector3 lookDirection = attackManager.cineCam.transform.forward;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            attackManager.transform.rotation = Quaternion.Slerp(attackManager.transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        if (attackManager.movementManager.currentState != attackManager.movementManager.idleState)
            attackManager.anim.SetBool("FullAiming", false);

        if (inputManager.attackAction.WasReleasedThisFrame())
            attackManager.ChangeState(attackManager.attackState);
        else if (inputManager.aimAction.WasReleasedThisFrame())
            attackManager.ChangeState(attackManager.idleState);
    }

    public override void ExitState()
    {
        attackManager.anim.SetBool("FullAiming", false);
        attackManager.anim.SetBool("Aiming", false);
        attackManager.cameraManager.MoveCamera(false);
    }
}
