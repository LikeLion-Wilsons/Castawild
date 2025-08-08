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

    [Networked] protected bool IsTriggerSet { get; set; }
    [Networked, HideInInspector] public bool IsAnimationFinished { get; set; }

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        inputManager = GetComponent<PlayerInputManager>();
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
        player = GetComponent<Player>();
        playerController = GetComponent<PlayerController>();
    }

    public void SetInput(PlayerNetworkInputData inputData) => input = inputData;
    public void SetPrevInputButton(NetworkButtons _prevInputButtons) => prevInputButtons = _prevInputButtons;

    /// <summary>
    /// 트리거애니메이션 끝났을 때 트리거세팅 초기화
    /// </summary>
    public void Host_InitTriggerSet() => IsTriggerSet = false;
}