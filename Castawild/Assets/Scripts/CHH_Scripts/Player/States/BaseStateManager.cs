using Fusion;
using UnityEngine;

public class BaseStateManager : NetworkBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public Player player;
    [HideInInspector] public PlayerMoveManager moveManager;
    [HideInInspector] public PlayerToolManager toolManager;
    [HideInInspector] public PlayerCameraManager cameraManager;
    [HideInInspector] public PlayerInteractUI interactUI;

    public PlayerNetworkInputData input { get; private set; }
    public NetworkButtons prevInputButtons;

    [Networked, HideInInspector] public bool IsAnimationFinished { get; set; }

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        player = GetComponent<Player>();
        moveManager = GetComponent<PlayerMoveManager>();
        toolManager = GetComponent<PlayerToolManager>();
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
        interactUI = GetComponentInChildren<PlayerInteractUI>();
    }

    public void SetInput(PlayerNetworkInputData inputData) => input = inputData;
    public void SetPrevInputButton(NetworkButtons _prevInputButtons) => prevInputButtons = _prevInputButtons;
}