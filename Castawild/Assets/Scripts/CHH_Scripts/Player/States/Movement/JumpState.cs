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
        movementManager.CanLanding = false;

        if (movementManager.previousState == movementManager.idleState)
            movementManager.CurrentMoveAnimation = MoveAnimatoinState.IdleJump;

        else if (movementManager.previousState == movementManager.walkState
            || movementManager.previousState == movementManager.runState)
            movementManager.CurrentMoveAnimation = MoveAnimatoinState.RunJump;
    }

    public override void UpdateState()
    {
        if (movementManager.playerController.Grounded && movementManager.CanLanding)
        {
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
        movementManager.CanLanding = false;
    }
}
