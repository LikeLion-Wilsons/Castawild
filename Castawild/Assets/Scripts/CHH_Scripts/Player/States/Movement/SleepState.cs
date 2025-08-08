
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

        movementManager.playerController.RPC_FreezePosition(true);
        movementManager.player.RPC_TurnOffUI();
        movementManager.player.RPC_AttachCameraToHead(true);
    }

    public override void UpdateState()
    {
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.interactInput)
            && movementManager.CanWakeUp)
            movementManager.ChangeState(MovementState.Idle);
    }

    public override void ExitState()
    {
        movementManager.player.RPC_TurnOffUI();
        movementManager.player.RPC_AttachCameraToHead(false);
    }
}