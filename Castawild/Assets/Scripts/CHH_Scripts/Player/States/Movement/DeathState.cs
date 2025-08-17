
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
        {
            movementManager.player.inventory.ThrowAllItem();
            movementManager.player.Client_SleepDeadCameraTarget(true, false);
        }

        movementManager.Revived = false;
        movementManager.moveManager.Host_FreezePosition(true);

        movementManager.flagManager.Set(PlayerFlags.Death);
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        base.ExitState();
        movementManager.flagManager.Clear(PlayerFlags.Death);
        movementManager.player.Client_SleepDeadCameraTarget(false, false);

        movementManager.moveManager.Host_SetChangePosition(movementManager.player.RespawnPos);

        movementManager.Revived = true;
        movementManager.moveManager.Host_FreezePosition(false);
    }
}
