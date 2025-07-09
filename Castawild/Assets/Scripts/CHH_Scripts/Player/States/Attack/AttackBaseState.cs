
using UnityEngine;

public abstract class AttackBaseState : BaseState
{
    protected AttackStateManager attackManager;

    public AttackBaseState(AttackStateManager attackManger, PlayerInputManager _inputManager)
    {
        attackManager = attackManger;
        inputManager = _inputManager;
    }

    protected void LookForward()
    {
        Vector3 lookDir = attackManager.cam.transform.forward;
        lookDir.y = 0f;
        attackManager.transform.rotation = Quaternion.LookRotation(lookDir);
    }
}