using Fusion;
using UnityEngine;

public class BaseStateManager : NetworkBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public PlayerInputManager inputManager;
    [HideInInspector] public PlayerCameraManager cameraManager;
    [HideInInspector] public Player player;
    [HideInInspector] public PlayerController playerController;


    public PlayerNetworkInputData input { get; private set; }
    public NetworkButtons prevInputButtons;

    public BaseState currentState;

     protected bool IsTriggerSet { get; set; }
    [Networked, HideInInspector] public bool IsAnimationFinished { get; set; }

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        inputManager = GetComponent<PlayerInputManager>();
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
        player = GetComponent<Player>();
        playerController = GetComponent<PlayerController>();
    }

    public void ChangeState(BaseState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }

    public void SetInput(PlayerNetworkInputData inputData) => input = inputData;
    public void SetPrevInputButton(NetworkButtons _prevInputButtons) => prevInputButtons = _prevInputButtons;

    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_TriggerSet(bool isTriggerSet) => IsTriggerSet = isTriggerSet;
}