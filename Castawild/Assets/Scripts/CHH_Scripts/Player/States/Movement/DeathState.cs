using UnityEngine;
using UnityEngine.UIElements;

public class DeathState : MovementBaseState
{

    public DeathState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.CurrentMoveState = MoveAnimationState.Death;
        movementManager.player.CanMove = false;
        movementManager.Revived = false;
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        movementManager.playerController.SetPosition(movementManager.player.RespawnPos);
        movementManager.Revived = true;
        movementManager.isTriggerSet = false;
        movementManager.player.CanMove = true;
    }
}
