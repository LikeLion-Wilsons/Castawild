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

        movementManager.flagManager.Set(PlayerFlags.Jump);

        if (movementManager.HasInputAuthority)
            movementManager.player.Client_PlayLocalSound(Sound.Player_Jump);
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
        base.ExitState();
        movementManager.flagManager.Clear(PlayerFlags.Jump);
        movementManager.CanLanding = false;
    }
}
