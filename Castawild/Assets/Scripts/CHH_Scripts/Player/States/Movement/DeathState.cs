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
        movementManager.player.RPC_AttachCameraToHead(true);
        movementManager.Revived = false;
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        movementManager.player.RPC_AttachCameraToHead(false);
        movementManager.playerController.SetPosition(movementManager.player.RespawnPos);
        movementManager.Revived = true;
        movementManager.RPC_TriggerSet(false);
        movementManager.player.CanMove = true;
    }
}
