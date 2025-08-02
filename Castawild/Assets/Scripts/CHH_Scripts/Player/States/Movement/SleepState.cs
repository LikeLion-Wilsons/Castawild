
using UnityEngine;

public class SleepState : MovementBaseState
{
    public SleepState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.player.StopPlayer();

        movementManager.CurrentMoveState = MoveAnimationState.Sleep;
        movementManager.currentMoveType = MoveType.Idle;

        movementManager.CanWakeUp = false;

        if (movementManager.HasStateAuthority)
        {
            movementManager.player.RPC_TurnOffUI();
            movementManager.cameraManager.RPC_ApplySleepCameraView(true);
        }
    }

    public override void UpdateState()
    {
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.interactInput) && movementManager.CanWakeUp)
        {
            movementManager.ChangeState(movementManager.idleState);
        }
    }

    public override void ExitState()
    {
        if (movementManager.HasStateAuthority)
            movementManager.player.RPC_TurnOffUI();
    }
}