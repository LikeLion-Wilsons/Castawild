public class WalkState : MovementBaseState
{
    public WalkState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.anim.SetBool("Walking", true);
        movementManager.currentMoveSpeed = movementManager.walkSpeed;
        movementManager.player.currentMoveType = MoveType.Walk;
    }

    public override void UpdateState()
    {
        // Run
        if (movementManager.inputManager.sprintAction.IsPressed() && movementManager.player.currentAttackType != AttackType.Aim)
            movementManager.ChangeState(movementManager.runState);

        // Crouch
        else if (movementManager.inputManager.crouchAction.WasPressedThisFrame())
            movementManager.ChangeState(movementManager.crouchState);

        // Idle
        else if (movementManager.dir.magnitude < 0.1f)
            movementManager.ChangeState(movementManager.idleState);

        // Jump
        if (inputManager.jumpAction.WasPressedThisFrame())
        {
            movementManager.previousState = this;
            movementManager.ChangeState(movementManager.jumpState);
        }
    }

    public override void ExitState()
    {
        movementManager.anim.SetBool("Walking", false);
    }
}