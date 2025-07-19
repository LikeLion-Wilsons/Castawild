using Fusion;
using UnityEngine;

public enum ViewType { None, FirstPerson, ThirdPerson }
public enum MoveAnimationType { Idle, Walk, Run, CrouchIdle, CrouchWalk, IdleJump, RunJump }

public class PlayerNetworkManager : NetworkBehaviour
{
    private PlayerCameraManager cameraManager;

    [Networked] public MoveAnimationType CurrentMoveType { get; set; }
    public bool isSpawned = false;

    void Awake()
    {
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
    }

    public override void Spawned()
    {
        isSpawned = true;
        if (HasStateAuthority)
            CurrentMoveType = MoveAnimationType.Idle;
    }
}
