using Fusion;
using UnityEngine;

public enum MoveAnimationState { Idle, Walk, Run, CrouchIdle, CrouchWalk, IdleJump, RunJump }
public enum ToolAnimationState { Idle, Aim, FullAim, FullUse }

public class PlayerNetworkManager : NetworkBehaviour
{
    public bool isSpawned = false;
    [Networked] public MoveAnimationState CurrentMoveState { get; set; }
    [Networked] public ToolAnimationState CurrentToolUseState { get; set; }
    [Networked] public Vector2 MoveValue { get; set; }
    [Networked] public ToolType CurrentToolType { get; set; }
    [Networked] public bool ComboAttack { get; set; }
    [Networked] public bool IsAnimationFinished { get; set; }
    [Networked] public bool CanReceiveInput { get; set; }
    [Networked] public bool CanMove { get; set; }

    public override void Spawned()
    {
        isSpawned = true;
        CurrentToolType = ToolType.Fist;
    }

    /// <summary>
    /// 공격 무기 들고있는지 확인
    /// </summary>
    public bool HoldTool()
    {
        if (CurrentToolType == ToolType.Throw || CurrentToolType == ToolType.Fist || CurrentToolType == ToolType.Spear || CurrentToolType == ToolType.Sword)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 곡괭이/도끼 들고있는지 확인
    /// </summary>
    public bool HoldCraftingTool()
    {
        if (CurrentToolType == ToolType.Axe || CurrentToolType == ToolType.Pickaxe)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 조준가능한 도구인지 확인
    /// </summary>
    public bool HoldAimTool() => CurrentToolType == ToolType.Bow || CurrentToolType == ToolType.Throw;

}
