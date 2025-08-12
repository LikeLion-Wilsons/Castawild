using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AnimationTrigger : MonoBehaviour
{
    private Player player;
    private PlayerInteractManager playercontroller;
    private MovementStateManager movementManager;
    private ToolStateManager toolManager;
    private PlayerInteractUI interactUI;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        playercontroller = GetComponentInParent<PlayerInteractManager>();
        movementManager = GetComponentInParent<MovementStateManager>();
        toolManager = GetComponentInParent<ToolStateManager>();
        interactUI = transform.parent.GetComponentInChildren<PlayerInteractUI>();
    }

    public void ToolAnimationFinishTrigger()
    {
        toolManager.IsDecreased = false;
        toolManager.DecreaseToolDuration = false;
        toolManager.IsAnimationFinished = true;
    }
    public void ToolAnimationStartTrigger()
    {
        toolManager.IsAnimationFinished = false;
    }
    public void MoveAnimationFinishTrigger() => movementManager.IsAnimationFinished = true;
    public void MoveAnimationStartTrigger() => movementManager.IsAnimationFinished = false;
    public void CanLanding() => movementManager.CanLanding = true;
    public void ReceiveInput() => toolManager.CanReceiveInput = true;
    public void StopReceiveInput()
    {
        if (toolManager.CanComboAttack)
            toolManager.ComboAttack = true;
        toolManager.CanComboAttack = false;
        toolManager.CanReceiveInput = false;
    }

    public void Interact() => playercontroller.Client_Interact();

    public void Throw(int isArrow)
    {
        toolManager.Client_Throw(isArrow);

        if (isArrow == 1)
            toolManager.DecreaseToolDuration = true;
    }

    public void ActiveDeathUI()
    {
        if (movementManager.HasInputAuthority)
            interactUI.ActiveDeathUI(true);
    }

    public void StartHit()
    {
        //toolManager.Host_StartHit();
    }

    public void FinishHit()
    {
        //toolManager.Host_FinishHit();
    }
}
