
public class CrouchState : MovementBaseState
{
    public CrouchState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.CurrentMoveState = MoveAnimationState.CrouchIdle;
        movementManager.currentMoveSpeed = movementManager.crouchSpeed;
    }

    public override void UpdateState()
    {
        if (movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
            movementManager.CurrentMoveState = MoveAnimationState.CrouchWalk;
        else
            movementManager.CurrentMoveState = MoveAnimationState.CrouchWalk;

        if (movementManager.input.IsDown(PlayerNetworkInputData.sprintInput) && movementManager.HasEnoughStaminaToRun())
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
    }
}