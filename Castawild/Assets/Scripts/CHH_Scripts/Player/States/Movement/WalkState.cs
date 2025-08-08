
using UnityEngine;

public class WalkState : MovementBaseState
{
    public WalkState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.CurrentMoveAnimation = MoveAnimatoinState.Walk;
        movementManager.currentMoveSpeed = movementManager.walkSpeed;
    }

    public override void UpdateState()
    {
        // Run
        if (movementManager.input.IsDown(PlayerNetworkInputData.sprintInput) && movementManager.HasEnoughStaminaToRun()
            && movementManager.toolStateManager.CurrentToolState != ToolState.Aim)
            movementManager.ChangeState(MovementState.Run);

        // Crouch
        else if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.crouchInput))
            movementManager.ChangeState(MovementState.Crouch);

        // Idle
        else if (!movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
            movementManager.ChangeState(MovementState.Idle);

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