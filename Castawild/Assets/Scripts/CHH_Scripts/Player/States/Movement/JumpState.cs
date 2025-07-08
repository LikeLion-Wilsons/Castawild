using UnityEngine;

public class JumpState : MovementBaseState
{
    public JumpState(MovementStateManager _movementManager, PlayerInputManager _inputManager)
        : base(_movementManager, _inputManager)
    {
    }

    public override void EnterState()
    {
        if (movementManager.previousState == movementManager.idleState)
            movementManager.anim.SetTrigger("IdleJump");

        else if (movementManager.previousState == movementManager.walkState
            || movementManager.previousState == movementManager.runState)
            movementManager.anim.SetTrigger("RunJump");
    }

    public override void UpdateState()
    {
        if (movementManager.jumped && movementManager.IsGrounded())
        {
            movementManager.jumped = false;

            if (inputManager.moveInput.magnitude == 0f)
                movementManager.ChangeState(movementManager.idleState);
            else if (inputManager.sprintAction.IsPressed())
                movementManager.ChangeState(movementManager.runState);
            else
                movementManager.ChangeState(movementManager.walkState);
        }
    }

    public override void ExitState()
    {

    }
}
