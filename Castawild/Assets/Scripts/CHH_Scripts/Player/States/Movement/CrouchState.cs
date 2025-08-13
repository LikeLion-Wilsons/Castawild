
public class CrouchState : MovementBaseState
{
    public CrouchState(MovementStateManager _movementManager)
        : base(_movementManager)
    {
    }

    public override void EnterState()
    {
        movementManager.CurrentMoveAnimation = MoveAnimatoinState.CrouchIdle;
        movementManager.moveManager.currentMoveSpeed = movementManager.crouchSpeed;
    }

    public override void UpdateState()
    {
        if (movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
            movementManager.CurrentMoveAnimation = MoveAnimatoinState.CrouchWalk;
        else
            movementManager.CurrentMoveAnimation = MoveAnimatoinState.CrouchWalk;

        if (movementManager.input.IsDown(PlayerNetworkInputData.sprintInput) && movementManager.All_CanRun())
            movementManager.Host_ChangeState(MovementState.Run);

        else if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.crouchInput))
        {
            if (!movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
                movementManager.Host_ChangeState(MovementState.Idle);
            else
                movementManager.Host_ChangeState(MovementState.Walk);
        }
    }

    public override void ExitState()
    {
    }
}