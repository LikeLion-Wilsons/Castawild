using UnityEngine;

public class JumpState : MovementBaseState
{
    public JumpState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.JumpTriggered = true;

        if (movementManager.previousState == movementManager.idleState)
            movementManager.CurrentMoveAnimation = MoveAnimatoinState.IdleJump;

        else if (movementManager.previousState == movementManager.walkState
            || movementManager.previousState == movementManager.runState)
            movementManager.CurrentMoveAnimation = MoveAnimatoinState.RunJump;
    }

    public override void UpdateState()
    {
        if (movementManager.isJumping && movementManager.playerController.Grounded)
        {
            movementManager.isJumping = false;

            if (!movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
                movementManager.Host_ChangeState(MovementState.Idle);
            else if (movementManager.input.IsDown(PlayerNetworkInputData.sprintInput) && movementManager.All_HasEnoughStaminaToRun())
                movementManager.Host_ChangeState(MovementState.Run);
            else
                movementManager.Host_ChangeState(MovementState.Walk);
        }
    }

    public override void ExitState()
    {
        movementManager.CanJump = true;
        movementManager.RPC_TriggerSet(false);
    }
}
