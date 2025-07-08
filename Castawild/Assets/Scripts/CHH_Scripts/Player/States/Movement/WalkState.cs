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
        if (movementManager.inputManager.sprintAction.IsPressed() && movementManager.player.currentAttackType != AttackType.Aim)
            ExitState(movementManager.runState);
        else if (movementManager.inputManager.crouchAction.WasPressedThisFrame())
            ExitState(movementManager.crouchState);
        else if (movementManager.dir.magnitude < 0.1f)
            ExitState(movementManager.idleState);
    }

    void ExitState(MovementBaseState state)
    {
        movementManager.anim.SetBool("Walking", false);
        movementManager.ChangeState(state);
    }
}