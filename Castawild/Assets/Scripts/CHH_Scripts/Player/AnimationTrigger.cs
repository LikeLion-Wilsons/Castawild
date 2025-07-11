using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    private MovementStateManager movementManager;
    [HideInInspector] public bool isAnimationFinished = false;
    [HideInInspector] public bool canReceiveInput = false;
    [HideInInspector] public bool canComboAttack = false;

    private void Awake()
    {
        movementManager = GetComponentInParent<MovementStateManager>();
    }

    public void AnimationFinishTrigger() => isAnimationFinished = true;
    public void AnimationStartTrigger() => isAnimationFinished = false;
    public void JumpForce() => movementManager.velocity.y += movementManager.jumpForce;
    public void Jumped() => movementManager.jumped = true;
    public void ReceiveInput() => canReceiveInput = true;
    public void StopReceiveInput()
    {
        if (canComboAttack)
            movementManager.anim.SetBool("ComboAttack", true);
        canComboAttack = false;
        canReceiveInput = false;
    }
}
