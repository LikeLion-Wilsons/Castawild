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
        player.CurrentBedID = Object.Id;
        player.movementManager.ChangeState(player.movementManager.sleepState);

        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        playerController.kcc.SetPosition(sleepPos.position);
    }
}