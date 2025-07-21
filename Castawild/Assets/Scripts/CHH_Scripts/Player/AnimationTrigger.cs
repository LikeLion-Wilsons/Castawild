using Fusion;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    private CwPlayer player;
    private MovementStateManager movementManager;
    private ToolStateManager toolManager;
    private NetworkCharacterControllerCustom networkCharacterController;
    private PlayerNetworkManager networkManager;
    [HideInInspector] public bool canReceiveInput = false;
    [HideInInspector] public bool canComboAttack = false;

    private void Awake()
    {
        player = GetComponentInParent<CwPlayer>();
        movementManager = GetComponentInParent<MovementStateManager>();
        toolManager = GetComponentInParent<ToolStateManager>();
        networkCharacterController = GetComponentInParent<NetworkCharacterControllerCustom>();
        networkManager = GetComponentInParent<PlayerNetworkManager>();
    }

    public void ToolAnimationFinishTrigger() => toolManager.isAnimationFinished = true;
    public void ToolAnimationStartTrigger() => toolManager.isAnimationFinished = false;
    public void MoveAnimationFinishTrigger() => movementManager.isAnimationFinished = true;
    public void MoveAnimationStartTrigger() => movementManager.isAnimationFinished = false;
    public void JumpForce() => networkCharacterController.Jump();
    public void Jumped() => movementManager.jumped = true;
    public void ReceiveInput() => canReceiveInput = true;
    public void StopReceiveInput()
    {
        if (canComboAttack)
            networkManager.ComboAttack = true;
        canComboAttack = false;
        canReceiveInput = false;
    }

    public void ApplyTool() => player.ApplyTool();
}
