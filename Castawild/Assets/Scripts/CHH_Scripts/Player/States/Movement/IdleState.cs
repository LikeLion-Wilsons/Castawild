
using UnityEngine;

public class IdleState : MovementBaseState
{
    public IdleState(MovementStateManager _movementManager)
        : base(_movementManager)
    {
    }

    public override void EnterState()
    {
        movementManager.CurrentMoveAnimation = MoveAnimatoinState.Idle;
        movementManager.flagManager.Set(PlayerFlags.MoveIdle);
    }

    public override void UpdateState()
    {
        if (movementManager.player.IsUIOpen)
            return;

        // Move
        if (movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
        {
            if (movementManager.input.IsDown(PlayerNetworkInputData.sprintInput) && movementManager.All_CanRun())
                movementManager.Host_ChangeState(MovementState.Run);
            else
                movementManager.Host_ChangeState(MovementState.Walk);
        }

        // Crouch
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.crouchInput))
            movementManager.Host_ChangeState(MovementState.Crouch);

        // Jump
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.jumpInput))
        {
            if (movementManager.moveManager.Grounded)
            {
                movementManager.previousState = MovementState.Idle;
                movementManager.Host_ChangeState(MovementState.Jump);
            }
        }
    }

    public override void ExitState()
    {
        movementManager.flagManager.Clear(PlayerFlags.MoveIdle);
    }
}