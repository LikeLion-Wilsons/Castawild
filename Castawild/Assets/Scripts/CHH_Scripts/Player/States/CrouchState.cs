
public class CrouchState : MovementBaseState
{
    public override void EnterState(MovementStateManager movement)
    {
        movement.anim.SetBool("Crouching", true);
        movement.currentMoveSpeed = movement.crouchSpeed;
    }

    public override void UpdateState(MovementStateManager movement)
    {
        if (movement.inputManager.sprintAction.IsPressed())
            ExitState(movement, movement.runState);
        else if (movement.inputManager.crouchAction.WasPressedThisFrame())
        {
            if (movement.dir.magnitude < 0.1f)
                ExitState(movement, movement.idleState);
            else
                ExitState(movement, movement.walkState);
        }
    }

    void ExitState(MovementStateManager movement, MovementBaseState state)
    {
        movement.anim.SetBool("Crouching", false);
        movement.SwitchState(state);
    }
}