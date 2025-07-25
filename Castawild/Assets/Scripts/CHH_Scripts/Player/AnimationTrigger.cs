using Fusion;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    private CwPlayer player;
    private MovementStateManager movementManager;
    private ToolStateManager toolManager;
    private PlayerNetworkManager networkManager;
    [HideInInspector] public bool canReceiveInput = false;
    [HideInInspector] public bool canComboAttack = false;

    private void Awake()
    {
        player = GetComponentInParent<CwPlayer>();
        movementManager = GetComponentInParent<MovementStateManager>();
        toolManager = GetComponentInParent<ToolStateManager>();
        networkManager = GetComponentInParent<PlayerNetworkManager>();
    }

    public void ToolAnimationFinishTrigger() => toolManager.isAnimationFinished = true;
    public void ToolAnimationStartTrigger() => toolManager.isAnimationFinished = false;
    public void MoveAnimationFinishTrigger() => movementManager.isAnimationFinished = true;
    public void MoveAnimationStartTrigger() => movementManager.isAnimationFinished = false;
    public void JumpForce()
    {
        movementManager.jumpTriggered = true;
        Debug.Log("movementManager.jumpTriggered " + movementManager.jumpTriggered);

    }
    public void Jumped() => movementManager.isJumping = true;
    public void ReceiveInput() => canReceiveInput = true;
    public void StopReceiveInput()
    {
        if (canComboAttack)
            toolManager.comboAttack = true;
        canComboAttack = false;
        canReceiveInput = false;
    }

    public void ApplyTool() => player.ApplyTool();
}
