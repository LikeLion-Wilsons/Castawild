using Fusion;
using UnityEngine;

public enum MoveAnimationType { Idle, Walk, Run, CrouchIdle, CrouchWalk, IdleJump, RunJump }

public class PlayerNetworkManager : NetworkBehaviour
{
    [Networked] public MoveAnimationType CurrentMoveType { get; set; }
    public bool isSpawned = false;
    [Networked] public Vector2 MoveValue { get; set; }

    public override void Spawned()
    {
        isSpawned = true;
        CurrentMoveType = MoveAnimationType.Idle;
    }
}
