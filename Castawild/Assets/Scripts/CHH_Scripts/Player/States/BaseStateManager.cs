using Fusion;
using UnityEngine;

public class BaseStateManager : NetworkBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public PlayerInputManager inputManager;
    [HideInInspector] public PlayerCameraManager cameraManager;
    [HideInInspector] public CwPlayer player;

    public PlayerNetworkInputData input { get; private set; }
    public NetworkButtons prevInputButtons;

    public BaseState currentState;

    [Networked] public bool IsAnimationFinished { get; set; }

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

    public void SetInput(PlayerNetworkInputData inputData) => input = inputData;
    public void SetPrevInputButton(NetworkButtons _prevInputButtons) => prevInputButtons = _prevInputButtons;

    //[Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    //public void RPC_SetAnimationFinished(bool isFinished)
    //{
    //    IsAnimationFinished = isFinished;
    //    Debug.Log($"애니메이션 완료 상태: {isFinished}");
    //}
}