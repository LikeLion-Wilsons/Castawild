public class AimState : AimBaseState
{
    public AimState(AimStateManger _aimManager, PlayerInputManager _inputManager)
    : base(_aimManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        if (aimManager.movementManager.currentState == aimManager.movementManager.idleState)
            aimManager.anim.SetBool("FullAiming", true);
        else
            aimManager.anim.SetBool("Aiming", true);
    }

    public override void UpdateState()
    {
        if (inputManager.aimAction.WasReleasedThisFrame())
            aimManager.SwitchState(aimManager.attackState);
    }
}