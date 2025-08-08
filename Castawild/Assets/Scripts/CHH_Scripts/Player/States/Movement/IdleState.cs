
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
        if (movementManager.input.IsDown(PlayerNetworkInputData.moveInput) && movementManager.player.All_CanMoving())
        {
            if (movementManager.input.IsDown(PlayerNetworkInputData.sprintInput) && movementManager.All_HasEnoughStaminaToRun()
                && movementManager.toolStateManager.CurrentToolState != ToolState.Aim)
                movementManager.Host_ChangeState(MovementState.Run);
            else
                movementManager.Host_ChangeState(MovementState.Walk);
        }

        // Crouch
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.crouchInput))
            movementManager.Host_ChangeState(MovementState.Crouch);

        // Jump
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.jumpInput) && movementManager.CanJump)
        {
            movementManager.CanJump = false;
            movementManager.previousState = this;
            movementManager.Host_ChangeState(MovementState.Jump);
        }
    }

    public override void ExitState()
    {

    }
}