using Unity.Cinemachine;
using UnityEngine;

public class BaseStateManager : MonoBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public PlayerInputManager inputManager;
    [HideInInspector] public CinemachineCamera cam;
    [HideInInspector] public CwPlayer player;

    public BaseState currentState;

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        inputManager = GetComponent<PlayerInputManager>();
        cam = GetComponentInChildren<CinemachineCamera>();
        player = GetComponent<CwPlayer>();
    }

    public void ChangeState(BaseState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
}