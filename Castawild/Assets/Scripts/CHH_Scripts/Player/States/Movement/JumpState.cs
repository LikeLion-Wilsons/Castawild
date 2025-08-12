using UnityEngine;

public class JumpState : MovementBaseState
{
    public JumpState(MovementStateManager _movementManager)
        : base(_movementManager)
    {
    }

    public override void EnterState()
    {
        movementManager.moveManager.JumpTriggered = true;
        movementManager.CanLanding = false;

        if (movementManager.previousState == MovementState.Idle)
            movementManager.CurrentMoveAnimation = MoveAnimatoinState.IdleJump;

        else if (movementManager.previousState == MovementState.Walk
            || movementManager.previousState == MovementState.Run)
            movementManager.CurrentMoveAnimation = MoveAnimatoinState.RunJump;
        Debug.Log("Enter Jump State");
    }

    public override void UpdateState()
    {
        if (movementManager.moveManager.Grounded && movementManager.CanLanding)
        {
            if (!movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
                movementManager.Host_ChangeState(MovementState.Idle);
            else if (movementManager.input.IsDown(PlayerNetworkInputData.sprintInput)
                 && movementManager.All_CanRun())
                movementManager.Host_ChangeState(MovementState.Run);
            else
                movementManager.Host_ChangeState(MovementState.Walk);
        }
    }

    public override void ExitState()
    {
        movementManager.CanLanding = false;
        Debug.Log("Exit Jump State");
    }
}
