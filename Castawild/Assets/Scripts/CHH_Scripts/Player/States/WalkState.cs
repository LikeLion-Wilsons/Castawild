public class WalkState : MovementBaseState
{
    public WalkState(MovementStateManager _movement, PlayerInputManager _inputManager)
        : base(_movement, _inputManager)
    {
    }

    public override void EnterState()
    {
        movement.anim.SetBool("Walking", true);
        movement.currentMoveSpeed = movement.walkSpeed;
    }

    public override void UpdateState()
    {
        if (movement.inputManager.sprintAction.IsPressed())
            ExitState(movement.runState);
        else if (movement.inputManager.crouchAction.WasPressedThisFrame())
            ExitState(movement.crouchState);
        else if (movement.dir.magnitude < 0.1f)
            ExitState(movement.idleState);
    }

    void ExitState(MovementBaseState state)
    {
        movement.anim.SetBool("Walking", false);
        movement.SwitchState(state);
    }
}