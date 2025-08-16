using Fusion;
using System;

[System.Flags]
public enum PlayerFlags : ushort
{
    None = 0,
    Aim = 1 << 0,
    UseTool = 1 << 1,
    Carry = 1 << 2,
    Eat = 1 << 3,
    MoveIdle = 1 << 4,
    Walk = 1 << 5,
    Run = 1 << 6,
    Jump = 1 << 7,
    Death = 1 << 8,
}

public sealed class PlayerFlagManager : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnFlagsChanged))]
    public PlayerFlags Flags { get; set; }

    public bool Has(PlayerFlags flags) => (Flags & flags) != 0;

    public void Set(PlayerFlags flags)
    {
        if (!HasStateAuthority)
            return;
        Flags |= flags;
    }

    public void Clear(PlayerFlags flags)
    {
        if (!HasStateAuthority)
            return;
        Flags &= ~flags;
    }

    public event Action<PlayerFlags> FlagsChanged;
    void OnFlagsChanged() => FlagsChanged?.Invoke(Flags);

    public bool IsAiming => (Flags & PlayerFlags.Aim) != 0;
    public bool IsUsingTool => (Flags & PlayerFlags.UseTool) != 0;
    public bool CanRun_Tool => (Flags & (PlayerFlags.Aim | PlayerFlags.Carry)) == 0;
    public bool IsMoveIdle => (Flags & PlayerFlags.MoveIdle) != 0;
    public bool IsRunning => (Flags & PlayerFlags.Run) != 0;
    public bool IsDead => (Flags & PlayerFlags.Death) != 0;
    public bool CanRecoverStamina => (Flags & (PlayerFlags.UseTool | PlayerFlags.Run | PlayerFlags.Jump | PlayerFlags.Eat)) == 0;

}
