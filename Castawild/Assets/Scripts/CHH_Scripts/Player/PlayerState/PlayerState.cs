public class PlayerState
{
    protected Player player;
    protected PlayerStateMachine playerStateMachine;

    public PlayerState(Player player, PlayerStateMachine stateMachinel, string animBoolName)
    {
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