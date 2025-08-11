
public class GatherState : MovementBaseState
{

    public GatherState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.anim.SetTrigger("Gather");
        movementManager.playerController.Host_FreezePosition(true);
    }

    public override void UpdateState()
    {
        if (movementManager.IsAnimationFinished)
            movementManager.Host_ChangeState(MovementState.Idle);
    }

    public override void ExitState()
    {
        movementManager.playerController.Host_FreezePosition(false);
    }
}
