using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AnimationTrigger : MonoBehaviour
{
    private Player player;
    private PlayerInteractManager playercontroller;
    private MovementStateManager movementManager;
    private ToolStateManager toolStateManager;
    private PlayerInteractUI interactUI;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        playercontroller = GetComponentInParent<PlayerInteractManager>();
        movementManager = GetComponentInParent<MovementStateManager>();
        toolStateManager = GetComponentInParent<ToolStateManager>();
        interactUI = transform.parent.GetComponentInChildren<PlayerInteractUI>();
    }

    public void ToolAnimationFinishTrigger()
    {
        toolStateManager.DecreaseToolDuration = false;
        toolStateManager.IsAnimationFinished = true;
    }
    public void ToolAnimationStartTrigger()
    {
        toolStateManager.IsAnimationFinished = false;
    }
    public void MoveAnimationFinishTrigger() => movementManager.IsAnimationFinished = true;
    public void MoveAnimationStartTrigger() => movementManager.IsAnimationFinished = false;
    public void CanLanding() => movementManager.CanLanding = true;
    public void ReceiveInput() => toolStateManager.CanReceiveInput = true;
    public void StopReceiveInput()
    {
        if (toolStateManager.CanComboAttack)
            toolStateManager.ComboAttack = true;
        toolStateManager.CanComboAttack = false;
        toolStateManager.CanReceiveInput = false;
    }

    public void Interact() => playercontroller.Client_Interact();

    public void Gather() => playercontroller.Client_Gather();

    public void Throw(int isArrow)
    {
        toolStateManager.Client_Throw(isArrow);

        if (isArrow == 1)
            toolStateManager.DecreaseToolDuration = true;
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

    public void Eat()
    {
        if (toolStateManager.HasStateAuthority)
            toolStateManager.player.Host_RestoreStatFromFood();
    }
}
