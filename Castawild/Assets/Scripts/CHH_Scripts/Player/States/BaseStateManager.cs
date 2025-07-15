using UnityEngine;

public class BaseStateManager : MonoBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public PlayerInputManager inputManager;
    [HideInInspector] public PlayerCameraManager cameraManager;
    [HideInInspector] public CwPlayer player;

    public BaseState currentState;

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        inputManager = GetComponent<PlayerInputManager>();
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
        player = GetComponent<CwPlayer>();
    }

    public void ChangeState(BaseState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
}