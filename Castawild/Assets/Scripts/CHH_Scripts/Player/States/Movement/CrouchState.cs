
public class CrouchState : MovementBaseState
{
    public CrouchState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.anim.SetBool("Crouching", true);
        movementManager.currentMoveSpeed = movementManager.crouchSpeed;
        movementManager.player.currentMoveType = MoveType.Crouch;
    }

    public override void UpdateState()
    {
        if (movementManager.inputManager.sprintAction.IsPressed())
            ExitState(movementManager.runState);
        else if (movementManager.inputManager.crouchAction.WasPressedThisFrame())
        {
            if (movementManager.dir.magnitude < 0.1f)
                ExitState(movementManager.idleState);
            else
                ExitState(movementManager.walkState);
        }
    }

    void ExitState(MovementBaseState state)
    {
        movementManager.anim.SetBool("Crouching", false);
        movementManager.ChangeState(state);
    }
}