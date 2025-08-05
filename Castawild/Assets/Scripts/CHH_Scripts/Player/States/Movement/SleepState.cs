
using UnityEngine;

public class SleepState : MovementBaseState
{
    public SleepState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        if (!movementManager.HasInputAuthority)
            Debug.Log("SleepEnter");
        movementManager.player.StopPlayer();

        movementManager.CurrentMoveState = MoveAnimationState.Sleep;
        movementManager.currentMoveType = MoveType.Idle;

        movementManager.CanWakeUp = false;

        if (movementManager.HasStateAuthority)
        {
            movementManager.player.RPC_TurnOffUI();
            movementManager.player.RPC_ApplySleepCameraView();
        }
    }

    public override void UpdateState()
    {
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.interactInput) && movementManager.CanWakeUp)
        {
            if (!movementManager.HasInputAuthority)
                Debug.Log("WakeUp");
            movementManager.ChangeState(movementManager.idleState);
        }
    }

    public override void ExitState()
    {
        if (!movementManager.HasInputAuthority)
            Debug.Log("SleepExit");
        if (movementManager.HasInputAuthority)
            movementManager.player.playerInteractUI.TurnOffUI();
    }
}