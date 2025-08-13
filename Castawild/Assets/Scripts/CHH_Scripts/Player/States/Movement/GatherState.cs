
using UnityEngine;

public class GatherState : MovementBaseState
{

    public GatherState(MovementStateManager _movementManager)
        : base(_movementManager)
    {
    }

    public override void EnterState()
    {
        if (movementManager.kneel)
            movementManager.anim.SetTrigger("Gather");
        else
            movementManager.anim.SetTrigger("Gather_Kneeling");
        movementManager.moveManager.Host_FreezePosition(true);
    }

    public override void UpdateState()
    {
        if (movementManager.IsAnimationFinished)
            movementManager.Host_ChangeState(MovementState.Idle);
    }

    public override void ExitState()
    {
        movementManager.moveManager.Host_FreezePosition(false);
        Debug.Log("Exit GatherState");
    }
}
