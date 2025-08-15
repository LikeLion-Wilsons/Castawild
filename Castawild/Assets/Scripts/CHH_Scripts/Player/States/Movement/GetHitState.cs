
using UnityEngine;

public class GetHitState : MovementBaseState
{
    public GetHitState(MovementStateManager _movementManager)
        : base(_movementManager)
    {
    }

    public override void EnterState()
    {
        movementManager.CurrentMoveAnimation = MoveAnimatoinState.GetHit;
        movementManager.moveManager.Host_FreezePosition(true);
    }

    public override void UpdateState()
    {
        if (movementManager.IsAnimationFinished)
            movementManager.Host_ChangeState(MovementState.Idle);
    }

    public override void ExitState()
    {
        base.ExitState();
        movementManager.moveManager.Host_FreezePosition(false);
    }
}