
using UnityEngine;

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

        movementManager.moveManager.Host_FreezePosition(true);

        movementManager.player.Client_TurnOffInteractiveUI();
        movementManager.player.Client_SleepDeadCameraTarget(true, true);

        if (movementManager.HasStateAuthority)
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
        movementManager.anim.SetTrigger("WakeUp");
        elapsed = 0f;
        movementManager.player.Client_TurnOffInteractiveUI();
        movementManager.player.Client_SleepDeadCameraTarget(false, true);

        if (movementManager.HasStateAuthority)
            movementManager.moveManager.Host_FreezePosition(false);

        movementManager.player.Host_FinishSleep();

        if (movementManager.HasStateAuthority)
            movementManager.Host_Sleep(false);
    }
}