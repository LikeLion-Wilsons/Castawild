using Unity.VisualScripting;

public class PlayerStateMachine
{
    public PlayerState currentState;

    public void Initialize(PlayerState newState)
    {
        currentState = newState;
        currentState.EnterState();
    }

    public void ChangeState(PlayerState newState)
    {
        if (currentState == newState)
            return;

        currentState.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
}