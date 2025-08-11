using Fusion;
using UnityEngine;

public class Bed : InteractableObject
{
    [Networked, HideInInspector] public bool CanSleep { get; set; } = true;
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
            player.movementManager.Host_ChangeState(MovementState.Sleep);
        }
        else
        {
            player.RPC_RequestCanSleep_Bed(this, false);
            player.RPC_RequestCurrentBed(this);
            player.movementManager.RPC_RequestChangeSleepState(playerRef);
        }

        player.All_SetRespawnPos(sleepPos.position);

        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        playerController.All_SetPosition(sleepPos.position);
    }
}