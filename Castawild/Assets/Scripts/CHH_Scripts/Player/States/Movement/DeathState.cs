
using UnityEngine;

public class DeathState : MovementBaseState
{
    public DeathState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.CurrentMoveState = MoveAnimationState.Death;
        movementManager.player.CanMove = true;
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        movementManager.isTriggerSet = false;
        movementManager.player.CanMove = false;
    }
}