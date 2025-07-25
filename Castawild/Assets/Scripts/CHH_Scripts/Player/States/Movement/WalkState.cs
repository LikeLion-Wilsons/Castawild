
using UnityEngine;

public class WalkState : MovementBaseState
{
    public WalkState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.networkManager.CurrentMoveState = MoveAnimationState.Walk;
        movementManager.currentMoveSpeed = movementManager.walkSpeed;
        movementManager.player.currentMoveType = MoveType.Walk;
    }

    public override void UpdateState()
    {
        // Run
        if (movementManager.input.IsDown(PlayerNetworkInputData.sprintInput) && movementManager.toolStateManager.currentState != movementManager.toolStateManager.aimState)
        {
            Debug.Log(movementManager.toolStateManager.currentState);
            movementManager.ChangeState(movementManager.runState);
        }

        // Crouch
        else if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.crouchInput))
            movementManager.ChangeState(movementManager.crouchState);

        // Idle
        else if (!movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
            movementManager.ChangeState(movementManager.idleState);

        // Jump
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.jumpInput) && movementManager.canJump)
        {
            movementManager.canJump = false;
            movementManager.previousState = this;
            movementManager.ChangeState(movementManager.jumpState);
        }
    }

    public override void ExitState()
    {
    }
}