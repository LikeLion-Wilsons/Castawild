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
        movementManager.CurrentMoveAnimation = MoveAnimatoinState.Death;
        movementManager.player.RPC_ApplyAttachCameraToHead(true);
        movementManager.Revived = false;
        movementManager.playerController.Host_FreezePosition(true);
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        movementManager.player.RPC_ApplyAttachCameraToHead(false);
        movementManager.playerController.Host_SetPosition(movementManager.player.RespawnPos);
        movementManager.Revived = true;
        movementManager.Host_InitTriggerSet();
        movementManager.playerController.Host_FreezePosition(false);
    }
}
