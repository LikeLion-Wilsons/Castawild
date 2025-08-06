
using UnityEngine;

public class GetHitState : MovementBaseState
{
    public GetHitState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.CurrentMoveState = MoveAnimationState.GetHit;
        movementManager.player.CanMove = false;
    }

    public override void UpdateState()
    {
        if (movementManager.IsAnimationFinished == true)
            movementManager.ChangeState(movementManager.idleState);
    }

    public override void ExitState()
    {
        movementManager.isTriggerSet = false;
        movementManager.player.CanMove = true;
    }
}