
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
        movementManager.CurrentMoveAnimation = MoveAnimatoinState.Sleep;

        movementManager.CanWakeUp = false;

        movementManager.playerController.Host_FreezePosition(true);

        movementManager.player.Client_TurnOffInteractiveUI();
        movementManager.player.Client_AttachCameraToHead(true);

        if (movementManager.HasStateAuthority)
            movementManager.Host_Sleep(true);
    }

    public override void UpdateState()
    {
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.interactInput)
            && movementManager.CanWakeUp)
            movementManager.Host_ChangeState(MovementState.Idle);
    }

    public override void ExitState()
    {
        movementManager.player.Client_TurnOffInteractiveUI();
        movementManager.player.Client_AttachCameraToHead(false);

        if (movementManager.HasStateAuthority)
            movementManager.Host_Sleep(false);
    }
}