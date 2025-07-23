using Fusion;
using UnityEngine;

public class BaseStateManager : MonoBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public PlayerInputManager inputManager;
    [HideInInspector] public PlayerCameraManager cameraManager;
    [HideInInspector] public CwPlayer player;
    [HideInInspector] public PlayerNetworkManager networkManager;

    public bool comboAttack;

    public PlayerNetworkInputData input { get; private set; }
    public NetworkButtons prevInputButtons;

    public bool isAnimationFinished = false;
    public BaseState currentState;

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        inputManager = GetComponent<PlayerInputManager>();
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
        player = GetComponent<CwPlayer>();
        networkManager = GetComponent<PlayerNetworkManager>();
    }

    public void ChangeState(BaseState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }

    public virtual void UpdateAnimationFlags()
    {
        networkManager.IsAnimationFinished = isAnimationFinished;
    }

    public void SetInput(PlayerNetworkInputData inputData) => input = inputData;
    public void SetPrevInputButton(NetworkButtons _prevInputButtons) => prevInputButtons = _prevInputButtons;
}