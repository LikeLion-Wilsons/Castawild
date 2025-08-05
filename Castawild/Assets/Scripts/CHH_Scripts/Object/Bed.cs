using Fusion;
using UnityEngine;

public class Bed : InteractableObject
{
    [Networked] private bool CanSleep { get; set; } = true;
    [SerializeField] private Transform sleepPos;

    private void Awake()
    {
        interactableType = InteractableType.Bed;
        isPlaceable = true;
    }

    public override bool CanInteract() => CanSleep;
    public void FinishSleep() => CanSleep = true;

    public override void Interact(PlayerRef playerRef)
    {
        CanSleep = false;
        NetworkObject playerObj = Runner.GetPlayerObject(playerRef);

        Player player = playerObj.GetComponent<Player>();
        player.currentBed = this;
        player.movementManager.RPC_ChangeSleepState(playerRef);

        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        playerController.RPC_SetPosition(sleepPos.position);
    }
}