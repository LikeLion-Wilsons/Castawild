using Fusion;
using UnityEngine;

public class AnimationTrigger : NetworkBehaviour
{
    private CwPlayer player;
    private MovementStateManager movementManager;
    private ToolStateManager toolManager;
    [HideInInspector] public bool canReceiveInput = false;
    [HideInInspector] public bool canComboAttack = false;

    #region Network
    [Networked] public bool ComboAttack { get; set; }
    [Networked] public bool IsAnimationFinished { get; set; }
    [Networked] public bool CanReceiveInput { get; set; }
    #endregion

    private void Awake()
    {
        player = GetComponentInParent<CwPlayer>();
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

    public void ApplyTool() => player.ApplyTool();


}
