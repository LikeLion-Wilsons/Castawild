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
            ExitState(movementManager.runState);

        // Crouch
        else if (movementManager.inputManager.crouchAction.WasPressedThisFrame())
            ExitState(movementManager.crouchState);

        // Idle
        else if (movementManager.dir.magnitude < 0.1f)
            ExitState(movementManager.idleState);

        // Jump
        if (inputManager.jumpAction.WasPressedThisFrame())
        {
            movementManager.previousState = this;
            ExitState(movementManager.jumpState);
        }
    }

    void ExitState(MovementBaseState state)
    {
        movementManager.anim.SetBool("Walking", false);
        movementManager.ChangeState(state);
    }
}