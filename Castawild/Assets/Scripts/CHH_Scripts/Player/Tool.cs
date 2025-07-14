public abstract class Tool
{
    protected CwPlayer player;

    protected Tool(CwPlayer _player)
    {
        player = _player;
    }

    public abstract void Attack();
}

public class Fist : Tool
{
    public Fist(CwPlayer _player) : base(_player)
    {
    }

    public override void Attack()
    {
    }
}

public class Throw : Tool
{
    public Throw(CwPlayer _player) : base(_player)
    {
    }

    public override void Attack()
    {
    }
}

public class Spear : Tool
{
    public Spear(CwPlayer _player) : base(_player)
    {
    }

    public override void Attack()
    {
    }
}

public class Sword : Tool
{
    public Sword(CwPlayer _player) : base(_player)
    {
    }

    public override void Attack()
    {
    }
}

public class Bow : Tool
{
    public Bow(CwPlayer _player) : base(_player)
    {
    }

    public override void Attack()
    {
    }
}

public class Axe : Tool
{
    public Axe(CwPlayer _player) : base(_player)
    {
    }

    public override void Attack()
    {
    }
}

public class Pickaxe : Tool
{
    public Pickaxe(CwPlayer _player) : base(_player)
    {
    }

    public override void Attack()
    {
    }
}

public class Knife : Tool
{
    public Knife(CwPlayer _player) : base(_player)
    {
    }

    public override void Attack()
    {
    }
}