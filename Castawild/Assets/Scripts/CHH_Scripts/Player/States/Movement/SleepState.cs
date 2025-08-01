
using UnityEngine;

public class SleepState : MovementBaseState
{
    public SleepState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.player.PlayerStop();

        movementManager.CurrentMoveState = MoveAnimationState.Sleep;
        movementManager.currentMoveType = MoveType.Idle;

        movementManager.player.interactableUI.alpha = 1f;
        movementManager.player.interactableText.text = "Wake Up";

        movementManager.cameraManager.SleepCamera(true);
    }

    public override void UpdateState()
    {
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.interactInput))
            movementManager.ChangeState(movementManager.idleState);
    }

    public override void ExitState()
    {
        movementManager.player.interactableUI.alpha = 0f;
        movementManager.cameraManager.SleepCamera(false);
    }
}