
using UnityEngine;

public class IdleState : MovementBaseState
{
    public IdleState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.CurrentMoveAnimation = MoveAnimatoinState.Idle;
    }

    public override void UpdateState()
    {
        // Move
        if (movementManager.input.IsDown(PlayerNetworkInputData.moveInput) && movementManager.player.CanMoving())
        {
            if (movementManager.input.IsDown(PlayerNetworkInputData.sprintInput) && movementManager.HasEnoughStaminaToRun()
                && movementManager.toolStateManager.CurrentToolState != ToolState.Aim)
                movementManager.ChangeState(MovementState.Run);
            else
                movementManager.ChangeState(MovementState.Walk);
        }

        // Crouch
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.crouchInput))
            movementManager.ChangeState(MovementState.Crouch);

        // Jump
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.jumpInput) && movementManager.CanJump)
        {
            movementManager.CanJump = false;
            movementManager.previousState = this;
            movementManager.ChangeState(MovementState.Jump);
        }
    }

    public override void ExitState()
    {

    }
}