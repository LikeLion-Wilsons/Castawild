using Fusion;
using UnityEngine;

public enum MoveAnimationState { Idle, Walk, Run, CrouchIdle, CrouchWalk, IdleJump, RunJump }
public enum ToolAnimationState { Idle, Aim, FullAim, Use, FullUse }

public class PlayerNetworkManager : NetworkBehaviour
{
    public bool isSpawned = false;
    [Networked] public MoveAnimationState CurrentMoveState { get; set; }
    [Networked] public ToolAnimationState CurrentToolUseState { get; set; }
    [Networked] public Vector2 MoveValue { get; set; }
    [Networked] public ToolType CurrentToolType { get; set; }
    [Networked] public bool ComboAttack { get; set; }
    [Networked] public bool IsAnimationFinished { get; set; }

    public override void Spawned()
    {
        isSpawned = true;
        CurrentToolType = ToolType.Fist;
    }
}
