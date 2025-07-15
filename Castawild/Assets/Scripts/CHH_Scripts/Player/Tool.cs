using UnityEngine;

public abstract class Tool
{
    protected CwPlayer player;

    protected Tool(CwPlayer _player)
    {
        player = _player;
    }

    public abstract void ApplyTool();
}

public class Fist : Tool
{
    public Fist(CwPlayer _player) : base(_player)
    {
    }

    public override void ApplyTool()
    {
    }
}

public class Throw : Tool
{
    private bool hasThrown = false;

    public Throw(CwPlayer _player) : base(_player)
    {
    }

    public override void ApplyTool()
    {
        GameObject throwObject = player.GetHoldToolObject();
        throwObject.GetComponent<ThrowObject>()?.Throw(player);
    }
}

public class Spear : Tool
{
    public Spear(CwPlayer _player) : base(_player)
    {
    }

    public override void ApplyTool()
    {
    }
}

public class Sword : Tool
{
    public Sword(CwPlayer _player) : base(_player)
    {
    }

    public override void ApplyTool()
    {
    }
}

public class Bow : Tool
{
    public Bow(CwPlayer _player) : base(_player)
    {
    }

    public override void ApplyTool()
    {
    }
}

public class Axe : Tool
{
    public Axe(CwPlayer _player) : base(_player)
    {
    }

    public override void ApplyTool()
    {
    }
}

public class Pickaxe : Tool
{
    public Pickaxe(CwPlayer _player) : base(_player)
    {
    }

    public override void ApplyTool()
    {
    }
}

public class Knife : Tool
{
    public Knife(CwPlayer _player) : base(_player)
    {
    }

    public override void ApplyTool()
    {
    }
}