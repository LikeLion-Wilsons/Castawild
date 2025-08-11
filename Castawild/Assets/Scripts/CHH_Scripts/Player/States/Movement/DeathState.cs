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

        if (movementManager.HasInputAuthority)
            movementManager.player.inventory.ThrowAllItem();

        if (movementManager.HasInputAuthority)
            movementManager.player.Client_SleepDeadCameraTarget(true, false);

        movementManager.Revived = false;
        movementManager.playerController.Host_FreezePosition(true);
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        if (movementManager.HasInputAuthority)
            movementManager.player.Client_SleepDeadCameraTarget(false, false);

        if (movementManager.HasStateAuthority)
            movementManager.playerController.Host_SetPosition(movementManager.player.RespawnPos);

        movementManager.Revived = true;
        movementManager.playerController.Host_FreezePosition(false);
    }
}
