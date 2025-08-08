
using UnityEngine;

public class GetHitState : MovementBaseState
{
    public GetHitState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.CurrentMoveAnimation = MoveAnimatoinState.GetHit;
        movementManager.playerController.Host_FreezePosition(true);
    }

    public override void UpdateState()
    {
        if (movementManager.IsAnimationFinished)
            movementManager.Host_ChangeState(MovementState.Idle);
    }

    public override void ExitState()
    {
        movementManager.IsAnimationFinished = false;
        movementManager.playerController.Host_FreezePosition(false);
    }
}