using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AnimationTrigger : MonoBehaviour
{
    private Player player;
    private PlayerController playercontroller;
    private MovementStateManager movementManager;
    private ToolStateManager toolManager;
    private PlayerCameraManager cameraManager;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        playercontroller = GetComponentInParent<PlayerController>();
        movementManager = GetComponentInParent<MovementStateManager>();
        toolManager = GetComponentInParent<ToolStateManager>();
        cameraManager = transform.parent.GetComponentInChildren<PlayerCameraManager>();
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

    public void Interact() => playercontroller.Interact();
    public void FinishSleep()
    {
        player.PlayerCanMove();

        if (player.HasInputAuthority)
            player.FinishSleep();
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

    public void Throw(int isArrow)
    {
        toolManager.SpawnThrowObject(isArrow == 0 ? false : true);

        if (isArrow == 0)
            player.CurrentToolActive(false);
        else
            player.arrow.SetActive(false);
    }

    public void SetTargetPos() => toolManager.SetTargetPos();
}
