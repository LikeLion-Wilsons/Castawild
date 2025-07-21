using Fusion;
using UnityEngine;

public enum MoveAnimationType { Idle, Walk, Run, CrouchIdle, CrouchWalk, IdleJump, RunJump }

public class PlayerNetworkManager : NetworkBehaviour
{
    private PlayerCameraManager cameraManager;

    [Networked] public MoveAnimationType CurrentMoveType { get; set; }
    public bool isSpawned = false;
    [Networked] public Vector2 MoveValue { get; set; }

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

    public override void FixedUpdateNetwork()
    {
        Debug.Log(CurrentMoveType);
    }
}
