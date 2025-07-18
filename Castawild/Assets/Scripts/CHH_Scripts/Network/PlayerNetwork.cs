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
    public override void FixedUpdateNetwork()
    {
        if (GetInput<PlayerNetworkInputData>(out var input))
        {
            Vector3 moveDir = movementManager.GetMoveDir(input.moveValue, HasInputAuthority);

            // 호스트 : 위치 & 상태 업데이트
            if (HasStateAuthority)
            {
                HandleMovement(input, moveDir);
                movementManager.RotatePlayer(moveDir);
            }
        }
    }

    private void HandleMovement(PlayerNetworkInputData input, Vector3 moveDir)
    {
        Vector3 moveVelocity = moveDir * movementManager.currentMoveSpeed;
        Vector3 gravityVelocity = movementManager.Gravity();

        Vector3 finalVelocity = new Vector3(moveVelocity.x, gravityVelocity.y, moveVelocity.z);

        controller.Move(finalVelocity * Time.fixedDeltaTime);

        movementManager.SetInput(input);
        toolManager.SetInput(input);

        movementManager.currentState.UpdateState();
        toolManager.currentState.UpdateState();

        movementManager.SetPrevInputButton(input.Buttons);
        toolManager.SetPrevInputButton(input.Buttons);
    }

    public override void Render()
    {
        movementManager.UpdateMoveAnimation();
    }
}
