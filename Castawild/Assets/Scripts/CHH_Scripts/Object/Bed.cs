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

    public override void Interact(PlayerRef playerRef)
    {
        if (!CanSleep)
            return;

        RPC_RequestTrySleep(playerRef);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestTrySleep(PlayerRef playerRef)
    {
        NetworkObject playerObj = Runner.GetPlayerObject(playerRef);
        if (playerObj == null || !CanSleep)
            return;

        Player player = playerObj.GetComponent<Player>();
        MovementStateManager movementManager = playerObj.GetComponent<MovementStateManager>();
        PlayerMoveManager moveManager = playerObj.GetComponent<PlayerMoveManager>();
        if (player == null || movementManager == null || moveManager == null)
            return;

        CanSleep = false;
        player.Host_currentBed = this;

        movementManager.Host_ChangeState(MovementState.Sleep);
        player.Host_SetRespawnPos(sleepPos.position);
        moveManager.Host_SetPosition(sleepPos.position);
    }
}