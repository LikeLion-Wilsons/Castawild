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
        if (movementManager.inputManager.moveInput.magnitude > 0.1f)
        {
            if (movementManager.inputManager.sprintAction.IsPressed() && movementManager.player.currentAttackType != AttackType.Aim)
                movementManager.ChangeState(movementManager.runState);
            else
                movementManager.ChangeState(movementManager.walkState);
        }
        if (movementManager.inputManager.crouchAction.WasPressedThisFrame())
            movementManager.ChangeState(movementManager.crouchState);
    }
}