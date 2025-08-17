using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerMoveManager : NetworkBehaviour
{
    private SimpleKCC kcc;
    private Player player;
    private PlayerCameraManager cameraManager;
    private PlayerFlagManager flagManager;

    [Header("Movement")]
    [Networked, HideInInspector] public bool CanMove { get; set; } = true;
    public float jumpImpulse = 3f;
    public float rotationSpeed = 15f;

    [Header("Falling")]
    [SerializeField] private float maxGroundAngle = 45f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float fallThreshold = 3f;
    [SerializeField] private float fallDamagePerMeter = 10f;
    private float startY;
    private bool isFalling;

    [Header("Falling Time")]
    [SerializeField] private float fallingDeadTime = 5f;
    private float fallingElapsed;

    [Header("Ground Check")]
    public bool Grounded { get; set; }
    [Networked, HideInInspector] public bool Grounded_Physics { get; set; }
    [SerializeField] private Transform groundStartPoint;
    [SerializeField] private float groundDistance = 0.5f;

    private PlayerNetworkInputData input;
    [Networked, HideInInspector] public bool JumpTriggered { get; set; }
    [Networked, HideInInspector] public float currentMoveSpeed { get; set; }
    [Networked, HideInInspector] public bool IsChangePos { get; set; }
    [Networked, HideInInspector] public Vector3 ChangePos { get; set; }

    private void Awake()
    {
        kcc = GetComponent<SimpleKCC>();
        player = GetComponent<Player>();
        flagManager = GetComponent<PlayerFlagManager>();
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
    }

    public override void Spawned()
    {
        kcc.SetMaxGroundAngle(maxGroundAngle);
    }

    private void Update()
    {
        Grounded = Physics.Raycast(groundStartPoint.position, Vector3.down, out RaycastHit hit, groundDistance);
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<PlayerNetworkInputData>(out var input))
            return;

        player.input = input;
        this.input = input;

        if (HasStateAuthority)
        {
            Host_ChangePosition();

            Host_Falling();
        }

        if (flagManager.IsDead)
            return;

        if (!CanMove)
        {
            if (HasStateAuthority)
                Host_Gravity();
            return;
        }

        All_HandleMovement(input);
    }

    public void RotatePlayer()
    {
        Vector3 lookDirection = cameraManager.CurrenCam.transform.forward;
        lookDirection.y = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }

    public override void Render()
    {
        kcc.Render();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Grounded ? Color.green : Color.red;
        Gizmos.DrawLine(groundStartPoint.position, groundStartPoint.position + Vector3.down * groundDistance);
    }

    /// <summary>
    /// 움직일 수 있는지 확인
    /// </summary>
    public bool All_CanMoving() => CanMove && player.IsCursorLocked;

    private void Host_ChangePosition()
    {
        if (IsChangePos)
        {
            IsChangePos = false;
            kcc.SetPosition(ChangePos);
        }
    }

    private void Host_Falling()
    {
        Vector3 velocity = kcc.RealVelocity;
        Grounded_Physics = kcc.IsGrounded;

        // 떨어지기 시작
        if (!isFalling && velocity.y < -0.1f && !Grounded_Physics)
        {
            isFalling = true;
            startY = transform.position.y;
        }

        // 떨어지는 중
        if (isFalling && !Grounded_Physics)
        {
            fallingElapsed += Runner.DeltaTime;
            if (fallingElapsed > fallingDeadTime)
            {
                fallingElapsed = 0f;
                player.Host_TakeDamaged(false, 10000f);
            }
        }

        // 착지
        if (isFalling && Grounded_Physics)
        {
            isFalling = false;

            float endY = transform.position.y;
            float fallDistance = startY - endY;

            if (fallDistance > fallThreshold)
            {
                float damage = (fallDistance - fallThreshold) * fallDamagePerMeter;
                player.Host_TakeDamaged(false, damage);
                if (input.currentView == ViewType.FirstPerson)
                    player.RPC_ApplyShakeCamera(transform.up, 0.5f);
                else
                    player.RPC_ApplyShakeCamera(transform.up, 0.3f);
            }
        }
    }

    private void Host_Gravity()
    {
        Vector3 velocity = kcc.RealVelocity;
        velocity.x = 0;
        velocity.z = 0;
        velocity.y += gravity * Runner.DeltaTime;
        if (!Grounded)
            kcc.Move(velocity);
    }

    public void All_RotateForward(PlayerNetworkInputData input)
    {
        Quaternion target = Quaternion.LookRotation(input.camForward);
        kcc.SetLookRotation(Quaternion.Slerp(kcc.Transform.rotation, target, rotationSpeed * Runner.DeltaTime));
    }

    private void All_HandleMovement(PlayerNetworkInputData input)
    {
        All_Move(input.moveDir);
        All_Rotate(input);
    }

    private void All_Move(Vector3 direction)
    {
        direction = direction.normalized;

        Vector3 velocity = kcc.RealVelocity;

        if (kcc.IsGrounded && velocity.y < 0f)
            velocity.y = 0f;

        // 수평 속도
        Vector3 horizontalVel = direction * currentMoveSpeed;

        velocity.x = horizontalVel.x;
        velocity.z = horizontalVel.z;

        // 점프
        float jump = 0f;
        if (HasStateAuthority)
        {
            if (JumpTriggered)
            {
                JumpTriggered = false;
                jump = jumpImpulse;
            }
        }
        kcc.Move(velocity, jump);
    }

    private void All_Rotate(PlayerNetworkInputData input)
    {
        if (input.currentView == ViewType.FirstPerson)
        {
            Quaternion yaw = Quaternion.Euler(0, input.lookValue.x * cameraManager.sensitivity, 0);
            kcc.SetLookRotation(kcc.Transform.rotation * yaw);
        }
        else if (input.currentView == ViewType.ThirdPerson && (input.moveValue.sqrMagnitude > 0.001f || flagManager.IsAiming))
        {
            if (input.camForward == Vector3.zero)
                return;
            All_RotateForward(input);
        }
    }

    /// <summary>
    /// 리스폰 위치 변경
    /// </summary>
    public void Host_SetChangePosition(Vector3 position)
    {
        if (!HasStateAuthority)
            return;
        IsChangePos = true;
        ChangePos = position;
    }

    /// <summary>
    /// 위치 변경
    /// </summary>
    public void Host_SetPosition(Vector3 position)
    {
        if (HasStateAuthority)
        {
            IsChangePos = true;
            ChangePos = position;
        }
        else
            RPC_NotifySetPosition(position);
    }


    /// <summary>
    /// 위치 변경
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_NotifySetPosition(Vector3 position)
    {
        IsChangePos = true;
        ChangePos = position;
    }

    /// <summary>
    /// 위치 고정
    /// </summary>
    public void Host_FreezePosition(NetworkBool freeze)
    {
        if (!HasStateAuthority)
            return;

        if (freeze)
        {
            kcc.ResetVelocity();
            CanMove = false;
        }
        else
            CanMove = true;
    }
}