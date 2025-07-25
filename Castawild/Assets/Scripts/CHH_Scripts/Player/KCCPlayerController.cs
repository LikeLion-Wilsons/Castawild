using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class KCCPlayerController : NetworkBehaviour
{
    public SimpleKCC kcc;

    [Header("Movement")]
    public float gravity = -20f;
    public float jumpImpulse = 3f;
    public float acceleration = 10f;
    public float maxSpeed = 2f;
    public float rotationSpeed = 15f;

    private MovementStateManager movementManager;
    private ToolStateManager toolManager;
    private PlayerCameraManager cameraManager;
    private PlayerNetworkManager networkManager;
    public bool Grounded => kcc.IsGrounded;

    public override void Spawned()
    {
        InitComponents();
    }

    void InitComponents()
    {
        kcc = GetComponent<SimpleKCC>();

        movementManager = GetComponent<MovementStateManager>();
        movementManager.ChangeState(movementManager.idleState);

        toolManager = GetComponent<ToolStateManager>();
        toolManager.ChangeState(toolManager.idleState);

        cameraManager = GetComponentInChildren<PlayerCameraManager>();
        if (!HasInputAuthority)
            cameraManager.SetNetworkCamera();

        networkManager = GetComponent<PlayerNetworkManager>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!networkManager.isSpawned)
            return;

        if (GetInput<PlayerNetworkInputData>(out var input))
        {
            // 속도 조절
            maxSpeed = networkManager.CanMove ? movementManager.currentMoveSpeed : 0f;

            movementManager.SetInput(input);
            toolManager.SetInput(input);

            movementManager.currentState.UpdateState();
            toolManager.currentState.UpdateState();

            movementManager.UpdateAnimationFlags();
            toolManager.UpdateAnimationFlags();
            toolManager.ChangeCurrentTool();

            movementManager.SetPrevInputButton(input.Buttons);
            toolManager.SetPrevInputButton(input.Buttons);

            networkManager.MoveValue = input.moveValue;

            Move(input.moveDir);
            Rotate(input);
        }
    }

    public void Move(Vector3 direction)
    {
        direction = direction.normalized;

        Vector3 velocity = kcc.RealVelocity;

        if (kcc.IsGrounded && velocity.y < 0f)
            velocity.y = 0f;

        // 중력 
        velocity.y += gravity * Runner.DeltaTime;

        // 수평 속도
        Vector3 horizontalVel = new Vector3(velocity.x, 0f, velocity.z);

        if (direction == Vector3.zero)
            horizontalVel = Vector3.zero;
        else
        {
            horizontalVel = Vector3.ClampMagnitude(
                horizontalVel + direction * acceleration * Runner.DeltaTime,
                maxSpeed
            );
        }

        velocity.x = horizontalVel.x;
        velocity.z = horizontalVel.z;

        // 점프
        float jump = 0f;
        if (networkManager.JumpTriggered)
        {
            Debug.Log("KCC.JumpTriggered" + networkManager.JumpTriggered);
            movementManager.jumpTriggered = false;
            networkManager.JumpTriggered = false;
            jump = jumpImpulse;
        }

        // SimpleKCC.Move
        // 첫 번재 인자 : 이동 벡터(속도) -> moveDir * speed, 중력 포함해서 넣기
        // 두 번째 인자 : y축 점프 힘 -> 점프 눌렀을 때만 값넣기, 아니면 0
        // Move 함수의 ManualFixedUpdate 내부에서 DeltaTime 곱하기 때문에 여기서는 곱하지 말기
        kcc.Move(velocity, jump);
    }

    private void Rotate(PlayerNetworkInputData input)
    {
        if (input.currentView == ViewType.FirstPerson)
        {
            Quaternion yaw = Quaternion.Euler(0, input.lookValue.x * cameraManager.sensitivity, 0);
            kcc.SetLookRotation(kcc.Transform.rotation * yaw);
        }
        else if (input.currentView == ViewType.ThirdPerson && input.moveValue.sqrMagnitude > 0.001f)
        {
            Quaternion target = Quaternion.LookRotation(input.camForward);
            kcc.SetLookRotation(Quaternion.Slerp(kcc.Transform.rotation, target, rotationSpeed * Runner.DeltaTime));
        }
    }

    public override void Render()
    {
        kcc.Render();

        if (!networkManager.isSpawned)
            return;

        movementManager.UpdateMoveAnimation(Runner.DeltaTime);
        toolManager.UpdateMoveAnimation();
    }
}
