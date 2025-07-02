public class PlayerState
{
    protected Player player;
    protected PlayerStateMachine stateMachine;
    protected string animName;

    public PlayerState(Player _player, PlayerStateMachine _stateMachinel, string _animName)
    {
        player = _player;
        stateMachine = _stateMachinel;
        animName = _animName;
    }

    public virtual void EnterState()
    {
    }

    public virtual void UpdateState()
    {
    }

    public virtual void ExitState()
    {
    }
}