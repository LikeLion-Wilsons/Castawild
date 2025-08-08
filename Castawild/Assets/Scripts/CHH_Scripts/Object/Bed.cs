using Fusion;
using UnityEngine;

public class Bed : InteractableObject
{
    [Networked] public bool CanSleep { get; set; } = true;
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
        if (!CanSleep)
            return;

        NetworkObject playerObj = Runner.GetPlayerObject(playerRef);
        Player player = playerObj.GetComponent<Player>();

        if (playerObj.HasStateAuthority)
        {
            CanSleep = false;
            player.Host_currentBed = this;
        }
        else
        {
            player.RPC_CanSleep_Bed(this, false);
            player.RPC_CurrentBed(this);
        }

        player.SetRespawnPos(sleepPos.position);
        player.movementManager.RPC_RequestChangeSleepState(playerRef);

        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        playerController.RPC_SetPosition(sleepPos.position);
    }
}