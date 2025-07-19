using Fusion;
using UnityEngine;

public enum ViewType { None, FirstPerson, ThirdPerson }
public enum MoveAnimationType { Idle, Walk, Run, CrouchIdle, CrouchWalk, IdleJump, RunJump }

public class PlayerNetworkManager : NetworkBehaviour
{
    private CwPlayer player;
    private CharacterController controller;
    private MovementStateManager movementManager;
    private ToolStateManager toolManager;
    private PlayerCameraManager cameraManager;

    [Networked] public MoveAnimationType CurrentMoveType { get; set; }
    public bool isSpawned = false;

    void Awake()
    {
        player = GetComponent<CwPlayer>();
        controller = GetComponent<CharacterController>();
        movementManager = GetComponent<MovementStateManager>();
        toolManager = GetComponent<ToolStateManager>();
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
    }

    public override void Spawned()
    {
        isSpawned = true;
        if (!HasInputAuthority)
            cameraManager.SetNetworkCamera();
        if (HasStateAuthority)
            CurrentMoveType = MoveAnimationType.Idle;
    }

    // HasInputAuthority : 내가 조종하는 캐릭터인가
    // HasStateAuthority : 캐릭터의 위치를 결정하는 주인인가 -> 호스트인가
    // HasInputAuthority && HasStateAuthority : 호스트의 자기 캐릭터
    // HasInputAuthority && !HasStateAuthority : 클라이언트의 자기 캐릭터
    // !HasInputAuthority && HasStateAuthority : 호스트의 다른 유저 캐릭터

    // FixedUpdateNetwork : HasInputAuthority || HasStateAuthority 일 때 실행 -> 입력처리, 이동 처리 등
    // LateUpdateNetwork : 모두 실행 -> 위치보간같은 최종처리
    // Render : 모든 클라 - 매 프레임 -> 애니메이션같은 시각효과 처리



}
