using Fusion;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    private CwPlayer player;
    private MovementStateManager movementManager;
    private NetworkCharacterControllerCustom networkCharacterController;
    [HideInInspector] public bool isAnimationFinished = false;
    [HideInInspector] public bool canReceiveInput = false;
    [HideInInspector] public bool canComboAttack = false;

    private void Awake()
    {
        player = GetComponentInParent<CwPlayer>();
        movementManager = GetComponentInParent<MovementStateManager>();
        networkCharacterController = GetComponentInParent<NetworkCharacterControllerCustom>();
    }

    public void AnimationFinishTrigger() => isAnimationFinished = true;
    public void AnimationStartTrigger() => isAnimationFinished = false;
    public void JumpForce() => networkCharacterController.Jump();
    public void Jumped() => movementManager.jumped = true;
    public void ReceiveInput() => canReceiveInput = true;
    public void StopReceiveInput()
    {
        if (canComboAttack)
            movementManager.anim.SetBool("ComboAttack", true);
        canComboAttack = false;
        canReceiveInput = false;
    }

    public void ApplyTool() => player.ApplyTool();
}
