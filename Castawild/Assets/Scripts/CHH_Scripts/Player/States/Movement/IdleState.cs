
public class IdleState : MovementBaseState
{
    public IdleState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.player.currentMoveType = MoveType.Idle;
    }

    public override void UpdateState()
    {
        // Move
        if (movementManager.input.IsDown(PlayerNetworkInputData.moveInput) && movementManager.canMove)
        {
            if (movementManager.input.IsDown(PlayerNetworkInputData.sprintInput) && movementManager.player.currentAttackType != AttackType.Aim)
                movementManager.ChangeState(movementManager.runState);
            else
                movementManager.ChangeState(movementManager.walkState);
        }

        // Crouch
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.crouchInput))
            movementManager.ChangeState(movementManager.crouchState);

        // Jump
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.jumpInput) && movementManager.canJump)
        {
            movementManager.canJump = false;
            movementManager.previousState = this;
            movementManager.ChangeState(movementManager.jumpState);
        }
    }

    public override void ExitState()
    {

    }
}