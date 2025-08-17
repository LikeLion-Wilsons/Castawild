
public class SleepState : MovementBaseState
{
    float elapsed = 0f;
    float canWakeUpTime = 1f;

    public SleepState(MovementStateManager _movementManager)
        : base(_movementManager)
    {
    }

    public override void EnterState()
    {
        movementManager.CurrentMoveAnimation = MoveAnimatoinState.Sleep;

        if (movementManager.HasInputAuthority)
        {
            int randomNumber = UnityEngine.Random.Range(0, 1);
            Sound[] SleepSounds = { Sound.Player_Sleep3, Sound.Player_Sleep3 };
            movementManager.player.Client_PlayLocalSound(SleepSounds[randomNumber]);
        }

        movementManager.moveManager.Host_FreezePosition(true);

        if (movementManager.HasStateAuthority)
            movementManager.player.RPC_ApplyTurnOffInteractiveUI();
        movementManager.player.Client_SleepDeadCameraTarget(true, true);

        movementManager.Host_Sleep(true);

        elapsed = 0f;
    }

    public override void UpdateState()
    {
        elapsed += movementManager.Runner.DeltaTime;
        if (elapsed < canWakeUpTime)
            return;

        movementManager.interactManager.RPC_ApplySetWakeUpUI();

        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.interactInput))
            movementManager.Host_ChangeState(MovementState.Idle);
    }

    public override void ExitState()
    {
        base.ExitState();
        movementManager.anim.SetTrigger("WakeUp");
        elapsed = 0f;
        if (movementManager.HasStateAuthority)
            movementManager.player.RPC_ApplyTurnOffInteractiveUI();
        movementManager.player.Client_SleepDeadCameraTarget(false, true);

        movementManager.moveManager.Host_FreezePosition(false);

        movementManager.player.Host_FinishSleep();

        movementManager.Host_Sleep(false);
    }
}