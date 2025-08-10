
using UnityEngine;

// StandToSleep 애니메이션 : SleepState Enter하면서 재생
// SleepToStand 애니메이션 : SleepState Exit하면서 재생 -> 애니메이션 재생중일 땐 Idle 상태
public class SleepState : MovementBaseState
{
    public SleepState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.anim.SetTrigger("Sleep");

        movementManager.playerController.Host_FreezePosition(true);

        movementManager.player.Client_TurnOffInteractiveUI();
        movementManager.player.Client_SleepDeadCameraTarget(true, true);

        if (movementManager.HasInputAuthority)
            movementManager.player.playerInteractUI.SetWakeUpUI();
        if (movementManager.HasStateAuthority)
            movementManager.Host_Sleep(true);
    }

    public override void UpdateState()
    {
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.interactInput))
            movementManager.Host_ChangeState(MovementState.Idle);
    }

    public override void ExitState()
    {
        movementManager.anim.SetTrigger("WakeUp");

        movementManager.player.Client_TurnOffInteractiveUI();
        movementManager.player.Client_SleepDeadCameraTarget(false, true);

        if (movementManager.HasStateAuthority)
            movementManager.playerController.Host_FreezePosition(false);

        movementManager.player.Host_FinishSleep();

        if (movementManager.HasStateAuthority)
            movementManager.Host_Sleep(false);
    }
}