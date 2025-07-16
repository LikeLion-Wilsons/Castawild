
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
        if (movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
            movementManager.anim.SetBool("Walking", true);
        else
            movementManager.anim.SetBool("Walking", false);

        if (movementManager.input.IsDown(PlayerNetworkInputData.sprintInput))
            movementManager.ChangeState(movementManager.runState);
        else if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.crouchInput))
        {
            if (!movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
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