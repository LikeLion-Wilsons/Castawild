using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerController : NetworkBehaviour
{
    public SimpleKCC kcc;
    private Player player;
    private PlayerInteractUI playerInteractUI;
    private Rigidbody rigid;
    private MovementStateManager movementManager;
    private ToolStateManager toolManager;
    private PlayerCameraManager cameraManager;

    [Header("Movement")]
    public float jumpImpulse = 3f;
    public float maxSpeed = 2f;
    public float rotationSpeed = 15f;

    [Header("Falling")]
    [SerializeField] private float maxGroundAngle = 45f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float fallThreshold = 3f;
    [SerializeField] private float damagePerMeter = 10f;
    private float startY;
    private bool isFalling;
    [SerializeField] private float fallingDeadTime = 5f;
    private float fallingElapsed;
    [SerializeField] private Transform checkStartPoint;
    [SerializeField] private float checkDistance = 0.2f;
    [Networked] public bool Grounded_Physics { get; set; }
    public bool Grounded { get; set; }

    [Header("Interact")]
    [SerializeField] private float interactHeight = 10f;
    [SerializeField] private Transform thirdPersonInteractPos;
    [SerializeField] private float interactRadius = 1f;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private float kneelY = 6.8f;
    [HideInInspector] public EnvironmentObject Client_currentInteractObject;

    [Networked, HideInInspector] public Vector3 ChangePos { get; set; }
    [Networked, HideInInspector] public bool IsChangePos { get; set; }

    Collider[] _interactResult = new Collider[5];

    private NetworkButtons prevInputButtons;
    public event Action<int> Hit;

    public override void Spawned()
    {
        InitComponents();
        kcc.SetMaxGroundAngle(maxGroundAngle);
    }

    private void InitComponents()
    {
        kcc = GetComponent<SimpleKCC>();

        player = GetComponent<Player>();
        playerInteractUI = GetComponentInChildren<PlayerInteractUI>();
        movementManager = GetComponent<MovementStateManager>();
        movementManager.Host_ChangeState(MovementState.Idle);

        toolManager = GetComponent<ToolStateManager>();
        toolManager.Host_ChangeState(ToolState.Idle);

        cameraManager = GetComponentInChildren<PlayerCameraManager>();

        if (!HasInputAuthority)
            cameraManager.SetNetworkCamera();

        rigid = GetComponent<Rigidbody>();
        rigid.isKinematic = false;
    }

    private void Update()
    {
        // 테스트용
        if (HasInputAuthority && Input.GetKeyDown(KeyCode.H))
        {
            player.screenEffect.TakeDamageEffect(0f);
            player.RPC_RequestHeal();
        }

        Grounded = Physics.Raycast(checkStartPoint.position, Vector3.down, out RaycastHit hit, checkDistance);

        Vector3 start = checkStartPoint.position;
        Vector3 end = start + Vector3.down * checkDistance;

        Debug.DrawLine(start, end, Grounded ? Color.green : Color.red);
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Grounded ? Color.green : Color.red;
    //    Gizmos.DrawLine(checkStartPoint.position, checkStartPoint.position + Vector3.down * checkDistance);
    //}

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<PlayerNetworkInputData>(out var input))
            return;

        if (HasStateAuthority)
        {
            Host_ChangePosition();

            Host_Falling();
        }

        All_HandleState(input);

        if (!player.CanMove)
        {
            if (HasStateAuthority)
                Host_Gravity();
            return;
        }

        All_HandleMovement(input);

        if (HasInputAuthority && !player.inventory.canvasHolder.AnyUIOpen())
            Client_TestTryOverlap(input);

        prevInputButtons = input.Buttons;
    }

    public override void Render()
    {
        kcc.Render();

        movementManager.All_UpdateMoveAnimation(Runner.DeltaTime);
        toolManager.All_UpdateMoveAnimation();
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
                player.Host_TakeDamage(false, 10000f);
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
                float damage = (fallDistance - fallThreshold) * damagePerMeter;
                player.Host_TakeDamage(false, damage);
                if (movementManager.input.currentView == ViewType.FirstPerson)
                    RPC_ApplyShakeCamera(transform.up, 0.5f);
                else
                    RPC_ApplyShakeCamera(transform.up, 0.3f);
            }
        }
    }

    private void All_HandleState(PlayerNetworkInputData input)
    {
        movementManager.SetInput(input);
        toolManager.SetInput(input);

        if (movementManager.movementStateDict.TryGetValue(movementManager.CurrentMoveState, out var movementState))
            movementState.UpdateState();
        if (toolManager.toolStateDict.TryGetValue(toolManager.CurrentToolState, out var toolState))
            toolState.UpdateState();

        movementManager.SetPrevInputButton(input.Buttons);
        toolManager.SetPrevInputButton(input.Buttons);
    }

    private void Host_ChangePosition()
    {
        if (IsChangePos)
        {
            IsChangePos = false;
            kcc.SetPosition(ChangePos);
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

    private void All_HandleMovement(PlayerNetworkInputData input)
    {
        maxSpeed = player.All_CanMoving() ? movementManager.currentMoveSpeed : 0f;
        movementManager.MoveValue = input.moveValue;

        if (HasStateAuthority)
            Host_Move(input.moveDir);
        All_Rotate(input);
    }

    private void Host_Move(Vector3 direction)
    {
        direction = direction.normalized;

        Vector3 velocity = kcc.RealVelocity;

        if (kcc.IsGrounded && velocity.y < 0f)
            velocity.y = 0f;

        // 수평 속도
        Vector3 horizontalVel = direction * maxSpeed;

        velocity.x = horizontalVel.x;
        velocity.z = horizontalVel.z;

        // 점프
        float jump = 0f;
        if (movementManager.JumpTriggered)
        {
            movementManager.JumpTriggered = false;
            jump = jumpImpulse;
        }

        // SimpleKCC.Move
        // 첫 번재 인자 : 이동 벡터(속도) -> moveDir * speed, 중력 포함해서 넣기
        // 두 번째 인자 : y축 점프 힘 -> 점프 눌렀을 때만 값넣기, 아니면 0
        // Move 함수의 ManualFixedUpdate 내부에서 DeltaTime 곱하기 때문에 여기서는 곱하지 말기

        kcc.Move(velocity, jump);
    }

    private void All_Rotate(PlayerNetworkInputData input)
    {
        if (input.currentView == ViewType.FirstPerson)
        {
            Quaternion yaw = Quaternion.Euler(0, input.lookValue.x * cameraManager.sensivity, 0);
            kcc.SetLookRotation(kcc.Transform.rotation * yaw);
        }
        else if (input.currentView == ViewType.ThirdPerson && (input.moveValue.sqrMagnitude > 0.001f || toolManager.All_IsAiming()))
        {
            if (input.camForward == Vector3.zero)
                return;
            All_RotateForward(input);
        }
    }

    public void All_RotateForward(PlayerNetworkInputData input)
    {
        Quaternion target = Quaternion.LookRotation(input.camForward);
        kcc.SetLookRotation(Quaternion.Slerp(kcc.Transform.rotation, target, rotationSpeed * Runner.DeltaTime));
    }

    private void Client_TestTryOverlap(PlayerNetworkInputData input)
    {
        Camera cam = Camera.main;

        Vector3 origin = (input.currentView == ViewType.FirstPerson) ? cam.transform.position : thirdPersonInteractPos.position;
        Vector3 point1 = origin + cam.transform.forward * interactHeight;
        Vector3 point2 = origin;

        int hitCount = Runner.GetPhysicsScene().
            OverlapCapsule(point1, point2, interactRadius, _interactResult, interactLayer, QueryTriggerInteraction.UseGlobal);

        if (hitCount > 0)
        {
            for (int i = 0; i < hitCount; i++)
            {
                var interact = _interactResult[i];

                // 돌 / 나무
                if (_interactResult[i].TryGetComponent<EnvironmentObject>(out var interactable))
                {
                    if (interactable.CanInteract())
                    {
                        playerInteractUI.InteractUI(interactable.interactableType);
                        Client_currentInteractObject = interactable;

                        if (interactable.interactableType == InteractableType.Gatherable
                            && input.WasPressed(prevInputButtons, PlayerNetworkInputData.interactInput))
                        {
                            movementManager.RPC_RequestChangeGatherState(Object.InputAuthority);
                            playerInteractUI.SetInteractText("줍기");

                            float targetTopY = interact.bounds.max.y;
                            if (targetTopY - transform.position.y >= kneelY)
                                movementManager.RPC_RequestSetKneel(false);
                            else
                                movementManager.RPC_RequestSetKneel(true);
                        }
                        break;
                    }
                    else
                    {
                        playerInteractUI.InteractUI();
                        Client_currentInteractObject = null;
                    }
                }

                // 다른 오브젝트 
                else if (_interactResult[i].TryGetComponent<InteractableObject>(out var interactableObject))
                {
                    playerInteractUI.InteractUI(interactableObject.interactableType);
                    playerInteractUI.SetInteractText(interactableObject.text);

                    // 설치가능한 오브젝트
                    if (interactableObject.isPlaceable)
                    {
                        if (interactableObject.CanInteract()
                            && input.WasPressed(prevInputButtons, PlayerNetworkInputData.removeInput))
                        {
                            player.inventory.RPC_GetItem(interactableObject.itemIndex, 1);
                            RPC_DespawnObject(interactableObject.GetComponent<NetworkObject>());
                            return;
                        }
                    }

                    if (interactableObject.CanInteract()
                        && input.WasPressed(prevInputButtons, PlayerNetworkInputData.interactInput))
                    {
                        interactableObject.Interact(Object.InputAuthority);
                    }
                }
            }
        }
        else
        {
            playerInteractUI.InteractUI();
            Client_currentInteractObject = null;
        }

        Debug.DrawLine(point1, point2, Color.green, 1f);

        DebugDrawCircle(point1, cam.transform.forward, interactRadius, Color.green);
        DebugDrawCircle(point2, cam.transform.forward, interactRadius, Color.green);
    }

    private void DebugDrawCircle(Vector3 center, Vector3 normal, float radius, Color color, int segments = 20)
    {
        normal.Normalize();

        Vector3 basis1 = Vector3.Cross(normal, Vector3.up);
        if (basis1 == Vector3.zero)
            basis1 = Vector3.Cross(normal, Vector3.right);
        basis1.Normalize();
        Vector3 basis2 = Vector3.Cross(normal, basis1);

        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle0 = Mathf.Deg2Rad * (i * angleStep);
            float angle1 = Mathf.Deg2Rad * ((i + 1) * angleStep);

            Vector3 point0 = center + radius * (Mathf.Cos(angle0) * basis1 + Mathf.Sin(angle0) * basis2);
            Vector3 point1 = center + radius * (Mathf.Cos(angle1) * basis1 + Mathf.Sin(angle1) * basis2);

            Debug.DrawLine(point0, point1, color, 1f);
        }
    }

    /// <summary>
    /// 돌/나무 등 Interact UI가 바뀔 때 애니메이션 재생되면 호출되는 함수
    /// </summary>
    public void Client_Interact()
    {
        if (Client_currentInteractObject == null || !HasInputAuthority)
            return;

        int att = 0;
        if (Client_currentInteractObject.interactableType == InteractableType.Tree && Client_currentInteractObject.CanInteract())
        {
            att = player.All_GetToolAtt("Axe");
            Client_currentInteractObject?.Interact(Object.InputAuthority, att);
            toolManager.RPC_RequestDecreaseToolDuration(true);
        }
        else if (Client_currentInteractObject.interactableType == InteractableType.Stone && Client_currentInteractObject.CanInteract())
        {
            att = player.All_GetToolAtt("Pickaxe");
            Client_currentInteractObject?.Interact(Object.InputAuthority, att);
            toolManager.RPC_RequestDecreaseToolDuration(true);
        }

        else if (Client_currentInteractObject.interactableType == InteractableType.Gatherable && Client_currentInteractObject.CanInteract())
            Client_currentInteractObject?.Interact(Object.InputAuthority, att);

        if (att != 0)
        {
            Debug.Log("Hit Invoke");
            Hit?.Invoke(att);
        }
    }

    /// <summary>
    /// 리스폰 위치 변경
    /// </summary>
    public void Host_SetPosition(Vector3 position)
    {
        IsChangePos = true;
        ChangePos = position;
    }

    /// <summary>
    /// 위치 변경
    /// </summary>
    public void All_SetPosition(Vector3 position)
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
    /// 위치 고정
    /// </summary>
    public void Host_FreezePosition(NetworkBool freeze)
    {
        if (!HasStateAuthority)
            return;
        if (freeze)
        {
            kcc.ResetVelocity();
            player.CanMove = false;
        }
        else
            player.CanMove = true;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ApplyHitInvoke(int dmg)
    {
        Hit?.Invoke(dmg);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_DespawnObject(NetworkObject despawnObject) => Runner.Despawn(despawnObject);

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
    /// 카메라 쉐이크
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ApplyShakeCamera(Vector3 direction, float force) => cameraManager.ShakeCamera(direction, force);
}
