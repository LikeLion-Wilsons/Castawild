public class AttackState : AimBaseState
{
    public AttackState(AimStateManger _aimManager, PlayerInputManager _inputManager)
    : base(_aimManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        if (aimManager.movementManager.currentState == aimManager.movementManager.idleState)
            aimManager.anim.SetBool("FullAiming", false);
        else
            aimManager.anim.SetBool("Aiming", false);
    }

    public override void UpdateState()
    {
        if (inputManager.aimAction.IsPressed())
            aimManager.SwitchState(aimManager.aimState);
    }
}