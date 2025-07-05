
public class CrouchState : MovementBaseState
{
    public CrouchState(MovementStateManager _movement, PlayerInputManager _inputManager)
        : base(_movement, _inputManager)
    {
    }

    public override void EnterState()
    {
        movement.anim.SetBool("Crouching", true);
        movement.currentMoveSpeed = movement.crouchSpeed;
    }

    public override void UpdateState()
    {
        if (movement.inputManager.sprintAction.IsPressed())
            ExitState(movement.runState);
        else if (movement.inputManager.crouchAction.WasPressedThisFrame())
        {
            if (movement.dir.magnitude < 0.1f)
                ExitState(movement.idleState);
            else
                ExitState(movement.walkState);
        }
    }

    void ExitState(MovementBaseState state)
    {
        movement.anim.SetBool("Crouching", false);
        movement.SwitchState(state);
    }
}