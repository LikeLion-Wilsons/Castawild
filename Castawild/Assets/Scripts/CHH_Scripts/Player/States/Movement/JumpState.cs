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
            movementManager.networkManager.CurrentMoveType = MoveAnimationType.IdleJump;

        else if (movementManager.previousState == movementManager.walkState
            || movementManager.previousState == movementManager.runState)
            movementManager.networkManager.CurrentMoveType = MoveAnimationType.RunJump;
    }

    public override void UpdateState()
    {
        if (movementManager.jumped && movementManager.networkCharacterController.Grounded)
        {
            movementManager.jumped = false;

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
    }
}
