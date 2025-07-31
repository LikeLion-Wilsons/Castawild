using Fusion;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    private Player player;
    private PlayerController playercontroller;
    private MovementStateManager movementManager;
    private ToolStateManager toolManager;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        playercontroller = GetComponentInParent<PlayerController>();
        movementManager = GetComponentInParent<MovementStateManager>();
        toolManager = GetComponentInParent<ToolStateManager>();
    }

    public void ToolAnimationFinishTrigger() => toolManager.IsAnimationFinished = true;
    public void ToolAnimationStartTrigger() => toolManager.IsAnimationFinished = false;
    public void MoveAnimationFinishTrigger() => movementManager.IsAnimationFinished = true;
    public void MoveAnimationStartTrigger() => movementManager.IsAnimationFinished = false;
    public void JumpForce() => movementManager.JumpTriggered = true;

    public void Jumped() => movementManager.isJumping = true;
    public void ReceiveInput() => toolManager.CanReceiveInput = true;
    public void StopReceiveInput()
    {
        if (toolManager.CanComboAttack)
            toolManager.ComboAttack = true;
        toolManager.CanComboAttack = false;
        toolManager.CanReceiveInput = false;
    }

    public void Interact() => playercontroller.Interact();
    public void FinishSleep() => player.FinishSleep();
    public void FinishEat() => toolManager.IsAnimationFinished = true;
}
