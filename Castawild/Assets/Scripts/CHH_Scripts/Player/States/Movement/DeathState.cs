
public class DeathState : MovementBaseState
{

    public DeathState(MovementStateManager _movementManager)
        : base(_movementManager)
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
        movementManager.moveController.Host_FreezePosition(true);
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        if (movementManager.HasInputAuthority)
            movementManager.player.Client_SleepDeadCameraTarget(false, false);

        if (movementManager.HasStateAuthority)
            movementManager.moveController.Host_SetPosition(movementManager.player.RespawnPos);

        movementManager.Revived = true;
        movementManager.moveController.Host_FreezePosition(false);
    }
}
