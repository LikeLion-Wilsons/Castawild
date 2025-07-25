using UnityEngine;

public class JumpState : MovementBaseState
{
    public JumpState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        if (movementManager.previousState == movementManager.idleState)
            movementManager.networkManager.CurrentMoveState = MoveAnimationState.IdleJump;

        else if (movementManager.previousState == movementManager.walkState
            || movementManager.previousState == movementManager.runState)
            movementManager.networkManager.CurrentMoveState = MoveAnimationState.RunJump;
    }

    public override void UpdateState()
    {
        if (movementManager.isJumping && movementManager.playerController.Grounded)
        {
            movementManager.isJumping = false;

            if (!movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
                movementManager.ChangeState(movementManager.idleState);
            else if (movementManager.input.IsDown(PlayerNetworkInputData.sprintInput))
                movementManager.ChangeState(movementManager.runState);
            else
                movementManager.ChangeState(movementManager.walkState);
        }
    }

    public override void ExitState()
    {
        movementManager.canJump = true;
        movementManager.isTriggerSet = false;
    }
}
