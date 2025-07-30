using Fusion;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    private Player player;
    private MovementStateManager movementManager;
    private ToolStateManager toolManager;
    [HideInInspector] public bool canReceiveInput = false;
    [HideInInspector] public bool canComboAttack = false;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        movementManager = GetComponentInParent<MovementStateManager>();
        toolManager = GetComponentInParent<ToolStateManager>();
    }

    public void ToolAnimationFinishTrigger() => toolManager.IsAnimationFinished = true;
    public void ToolAnimationStartTrigger() => toolManager.IsAnimationFinished = false;
    public void MoveAnimationFinishTrigger() => movementManager.IsAnimationFinished = true;
    public void MoveAnimationStartTrigger() => movementManager.IsAnimationFinished = false;
    public void JumpForce() => movementManager.JumpTriggered = true;

    public void Jumped() => movementManager.isJumping = true;
    public void ReceiveInput() => canReceiveInput = true;
    public void StopReceiveInput()
    {
        if (canComboAttack)
            toolManager.ComboAttack = true;
        canComboAttack = false;
        canReceiveInput = false;
    }
}
