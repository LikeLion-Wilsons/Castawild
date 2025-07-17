using Fusion;
using UnityEngine;

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
        {
            cameraManager.firstPersonCam.gameObject.SetActive(false);
            cameraManager.thirdPersonCam.gameObject.SetActive(false);
        }
        if (HasStateAuthority)
        {
            CurrentMoveType = MoveAnimationType.Idle;
        }
    }

    // HasInputAuthority : 내가 조종하는 캐릭터인가
    // HasStateAuthority : 캐릭터의 위치를 결정하는 주인인가 -> 호스트인가
    // HasInputAuthority && HasStateAuthority : 호스트의 자기 캐릭터
    // HasInputAuthority && !HasStateAuthority : 클라이언트의 자기 캐릭터
    // !HasInputAuthority && HasStateAuthority : 호스트의 다른 유저 캐릭터
    public override void FixedUpdateNetwork()
    {
        if (GetInput<PlayerNetworkInputData>(out var input))
        {
            Vector3 moveDir = movementManager.GetMoveDir(input.moveValue);
            // 호스트 : 상태변경, 위치 이동
            if (HasStateAuthority)
            {
                HandleMovement(input, moveDir);
            }

            // 클라 : 애니메이션, 회전
            if (HasInputAuthority)
            {
                HandleLocalVisuals(input, moveDir);
            }

        }
        movementManager.UpdateMoveAnimation();
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

    private void HandleLocalVisuals(PlayerNetworkInputData input, Vector3 moveDir)
    {
        movementManager.RotatePlayer(moveDir);
    }

    public bool GetHasInputAuthority() => HasInputAuthority;
}
