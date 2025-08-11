using UnityEngine;
using static Unity.Collections.Unicode;

public class RunState : MovementBaseState
{
    public RunState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        movementManager.CurrentMoveAnimation = MoveAnimatoinState.Run;
        movementManager.currentMoveSpeed = movementManager.runSpeed;
        if (movementManager.HasInputAuthority)
            movementManager.cameraManager.run = true;
    }

    public override void UpdateState()
    {
        if (movementManager.Stamina <= 0)
        {
            movementManager.Host_ChangeState(MovementState.Walk);
            return;
        }
        else
            movementManager.Stamina -= movementManager.player.staminaRunDecreaseRate * movementManager.Runner.DeltaTime;

        // Walk
        if (movementManager.input.IsUp(PlayerNetworkInputData.sprintInput) || movementManager.Stamina <= 0)
            movementManager.Host_ChangeState(MovementState.Walk);

        // Idle
        else if (!movementManager.input.IsDown(PlayerNetworkInputData.moveInput))
            movementManager.Host_ChangeState(MovementState.Idle);

        // Crouch
        else if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.crouchInput))
            movementManager.Host_ChangeState(MovementState.Crouch);

        // Jump
        if (movementManager.input.WasPressed(movementManager.prevInputButtons, PlayerNetworkInputData.jumpInput) && movementManager.playerController.Grounded)
        {
            movementManager.previousState = this;
            movementManager.Host_ChangeState(MovementState.Jump);
        }
    }

    public override void ExitState()
    {
        if (movementManager.HasInputAuthority)
            movementManager.cameraManager.run = false;
    }
}