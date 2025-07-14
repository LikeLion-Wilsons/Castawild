
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
        if (inputManager.MoveInputDectected())
            movementManager.anim.SetBool("Walking", true);
        else if (!inputManager.MoveInputDectected())
            movementManager.anim.SetBool("Walking", false);

        if (movementManager.inputManager.sprintAction.IsPressed())
            movementManager.ChangeState(movementManager.runState);
        else if (movementManager.inputManager.crouchAction.WasPressedThisFrame())
        {
            if (movementManager.dir.magnitude < 0.1f)
                movementManager.ChangeState(movementManager.idleState);
            else
                movementManager.ChangeState(movementManager.walkState);
        }
    }

    public override void ExitState()
    {
        movementManager.anim.SetBool("Crouching", false);
    }
}