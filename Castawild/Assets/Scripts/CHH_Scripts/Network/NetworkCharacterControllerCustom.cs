namespace Fusion
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Unity.IO.LowLevel.Unsafe;
    using UnityEngine;

    [StructLayout(LayoutKind.Explicit)]
    [NetworkStructWeaved(WORDS + 4)]
    public unsafe struct NetworkCCDataCustom : INetworkStruct
    {
        public const int WORDS = NetworkTRSPData.WORDS + 4;
        public const int SIZE = WORDS * 4;

        [FieldOffset(0)]
        public NetworkTRSPData TRSPData;

        [FieldOffset((NetworkTRSPData.WORDS + 0) * Allocator.REPLICATE_WORD_SIZE)]
        int _grounded;

        [FieldOffset((NetworkTRSPData.WORDS + 1) * Allocator.REPLICATE_WORD_SIZE)]
        Vector3Compressed _velocityData;

        public bool Grounded
        {
            get => _grounded == 1;
            set => _grounded = (value ? 1 : 0);
        }

        public Vector3 Velocity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _velocityData;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _velocityData = value;
        }
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [NetworkBehaviourWeaved(NetworkCCDataCustom.WORDS)]
    public sealed unsafe class NetworkCharacterControllerCustom : NetworkTRSP, INetworkTRSPTeleport, IBeforeAllTicks, IAfterAllTicks, IBeforeCopyPreviousState
    {
        new ref NetworkCCDataCustom Data => ref ReinterpretState<NetworkCCDataCustom>();

        [Header("Character Controller Settings")]
        public float gravity = -20.0f;
        public float jumpImpulse = 8.0f;
        public float acceleration = 10.0f;
        public float maxSpeed = 2.0f;
        public float rotationSpeed = 15.0f;

        private Tick _initial;
        private CharacterController _controller;

        private MovementStateManager movementManager;
        private ToolStateManager toolManager;
        private PlayerCameraManager cameraManager;
        private PlayerNetworkManager networkManager;

        public Vector3 Velocity
        {
            get => Data.Velocity;
            set => Data.Velocity = value;
        }

        public bool Grounded
        {
            get => Data.Grounded;
            set => Data.Grounded = value;
        }

        public void Teleport(Vector3? position = null, Quaternion? rotation = null)
        {
            _controller.enabled = false;
            NetworkTRSP.Teleport(this, transform, position, rotation);
            _controller.enabled = true;
        }

        public void Jump(bool ignoreGrounded = false, float? overrideImpulse = null)
        {
            if (Data.Grounded || ignoreGrounded)
            {
                var newVel = Data.Velocity;
                newVel.y += overrideImpulse ?? jumpImpulse;
                Data.Velocity = newVel;
            }
        }

        public void Move(Vector3 direction)
        {
            var deltaTime = Runner.DeltaTime;
            var previousPos = transform.position;
            var moveVelocity = Data.Velocity;

            direction = direction.normalized;

            if (Data.Grounded && moveVelocity.y < 0)
                moveVelocity.y = 0f;

            // 중력 적용
            moveVelocity.y += gravity * Runner.DeltaTime;

            // 수평 이동
            var horizontalVel = default(Vector3);
            horizontalVel.x = moveVelocity.x;
            horizontalVel.z = moveVelocity.z;

            if (direction == default)
                horizontalVel = Vector3.zero;
            else
                // 방향을 기준으로 가속, 최대 속도 넘지 않게 Clamp
                horizontalVel = Vector3.ClampMagnitude(horizontalVel + direction * acceleration * deltaTime, maxSpeed);

            moveVelocity.x = horizontalVel.x;
            moveVelocity.z = horizontalVel.z;

            _controller.Move(moveVelocity * deltaTime);

            Data.Velocity = (transform.position - previousPos) * Runner.TickRate;
            Data.Grounded = _controller.isGrounded;
        }

        public override void Spawned()
        {
            _initial = default;
            TryGetComponent(out _controller);

            // CharacterController는 enabled = true 상태로 처음 시작할 때 이전 프레임의 위치를 내부적으로 가지고 있음
            // Fusion에서 Spawn된 직후 위치가 바뀔 수 있어서 초기 위치가 꼬이는 버그가 발생
            // => 이를 방지하기 위해 한 번 껐다가 다시 켜면서 캐시를 초기화
            _controller.enabled = false;
            _controller.enabled = true;

            CopyToBuffer();
            if (!HasInputAuthority)
                cameraManager.SetNetworkCamera();

            toolManager.ChangeState(toolManager.idleState);
            movementManager.ChangeState(movementManager.idleState);
        }

        public override void Render()
        {
            NetworkTRSP.Render(this, transform, false, false, false, ref _initial);

            if (!networkManager.isSpawned)
                return;
            movementManager.UpdateMoveAnimation(Runner.DeltaTime);
            toolManager.UpdateMoveAnimation();
        }

        // Tick 시작 전에 호출 -> 현재 시뮬레이션 상태를 Unity 오브젝트에 반영
        void IBeforeAllTicks.BeforeAllTicks(bool resimulation, int tickCount)
        {
            CopyToEngine();
        }

        // Tick 끝난 후 호출 -> Unity 오브젝트의 변경 내용을 네트워크 상태로 복사
        void IAfterAllTicks.AfterAllTicks(bool resimulation, int tickCount)
        {
            CopyToBuffer();
        }

        // State 복사 직전에 호출됨 -> 보간/롤백을 위한 상태 백업
        void IBeforeCopyPreviousState.BeforeCopyPreviousState()
        {
            CopyToBuffer();
        }

        void Awake()
        {
            TryGetComponent(out _controller);
            InitComponents();
        }

        // 현재 트랜스폼 상태를 네트워크 상태에 복사 -> HasStateAuthority 에서만 호출
        void CopyToBuffer()
        {
            Data.TRSPData.Position = transform.position;
            Data.TRSPData.Rotation = transform.rotation;
        }

        // 네트워크 상태를 현재 트랜스폼에 반영 -> 모두 호출
        void CopyToEngine()
        {
            // Unity CharacterController 특성상 위치를 강제로 바꿀 때 껐다 켜줘야 충돌 오류 안남
            _controller.enabled = false;

            transform.SetPositionAndRotation(Data.TRSPData.Position, Data.TRSPData.Rotation);

            _controller.enabled = true;
        }

        private void InitComponents()
        {
            movementManager = GetComponent<MovementStateManager>();
            toolManager = GetComponent<ToolStateManager>();
            cameraManager = GetComponentInChildren<PlayerCameraManager>();
            networkManager = GetComponent<PlayerNetworkManager>();
        }

        // HasInputAuthority || HasStateAuthority 일 때 실행
        public override void FixedUpdateNetwork()
        {
            if (!networkManager.isSpawned)
                return;

            // 서버에 보낸 Input값 가져오기 
            if (GetInput<PlayerNetworkInputData>(out var input))
            {
                if (networkManager.CanMove)
                    maxSpeed = movementManager.currentMoveSpeed;
                else
                    maxSpeed = 0f;
                movementManager.SetInput(input);
                toolManager.SetInput(input);

                toolManager.UpdateAnimationFlags();
                movementManager.currentState.UpdateState();
                toolManager.currentState.UpdateState();

                toolManager.ChangeCurrentTool();

                movementManager.SetPrevInputButton(input.Buttons);
                toolManager.SetPrevInputButton(input.Buttons);
            }

            networkManager.MoveValue = input.moveValue;
            Move(input.moveDir);
            Rotate(input);
        }

        private void Rotate(PlayerNetworkInputData input)
        {
            if (input.currentView == ViewType.FirstPerson)
                transform.Rotate(Vector3.up * input.lookValue.x * cameraManager.sensitivity);

            if (input.currentView == ViewType.ThirdPerson && input.moveValue.sqrMagnitude > 0.001f)
                transform.forward = input.camForward;
        }
    }
}