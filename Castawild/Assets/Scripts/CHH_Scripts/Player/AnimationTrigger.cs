using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AnimationTrigger : MonoBehaviour
{
    private Player player;
    private PlayerController playercontroller;
    private MovementStateManager movementManager;
    private ToolStateManager toolManager;
    private PlayerCameraManager cameraManager;
    private PlayerInteractUI interactUI;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        playercontroller = GetComponentInParent<PlayerController>();
        movementManager = GetComponentInParent<MovementStateManager>();
        toolManager = GetComponentInParent<ToolStateManager>();
        cameraManager = transform.parent.GetComponentInChildren<PlayerCameraManager>();
        interactUI = transform.parent.GetComponentInChildren<PlayerInteractUI>();
    }

    public void ToolAnimationFinishTrigger() => toolManager.IsAnimationFinished = true;
    public void ToolAnimationStartTrigger() => toolManager.IsAnimationFinished = false;
    public void MoveAnimationFinishTrigger() => movementManager.IsAnimationFinished = true;
    public void MoveAnimationStartTrigger() => movementManager.IsAnimationFinished = false;
    public void Jumped() => movementManager.isJumping = true;
    public void ReceiveInput() => toolManager.CanReceiveInput = true;
    public void StopReceiveInput()
    {
        if (toolManager.CanComboAttack)
            toolManager.ComboAttack = true;
        toolManager.CanComboAttack = false;
        toolManager.CanReceiveInput = false;
    }

    public void Interact() => playercontroller.Client_Interact();
    public void FinishSleep()
    {
        playercontroller.RPC_FreezePosition(false);
        player.All_FinishSleep();
    }

    public void CanWakeUp()
    {
        if (player.HasStateAuthority)
            movementManager.CanWakeUp = true;

        if (player.HasInputAuthority)
        {
            player.playerInteractUI.SetWakeUpUI();
            movementManager.isLyingOrGettingUp = false;
        }
    }

    public void LyingOrGettingUp(int playing) => movementManager.isLyingOrGettingUp = (playing != 0);

    public void Throw(int isArrow) => toolManager.Client_SetTargetPos(isArrow);

    public void ActiveDeathUI()
    {
        if (movementManager.HasInputAuthority)
            interactUI.ActiveDeathUI(true);
    }

    public void StartHit()
    {
        if (toolManager.HasStateAuthority)
            toolManager.Host_StartHit();
    }

    public void FinishHit()
    {
        if (toolManager.HasStateAuthority)
            toolManager.Host_FinishHit();
    }
}
