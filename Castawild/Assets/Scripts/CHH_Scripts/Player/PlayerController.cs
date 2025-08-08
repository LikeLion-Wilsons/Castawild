using Fusion;
using Fusion.Addons.SimpleKCC;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerController : NetworkBehaviour
{
    public SimpleKCC kcc;
    private Player player;
    private Rigidbody rigid;
    private MovementStateManager movementManager;
    private ToolStateManager toolManager;
    private PlayerCameraManager cameraManager;

    [Header("Movement")]
    public float jumpImpulse = 3f;
    public float maxSpeed = 2f;
    public float rotationSpeed = 15f;

    [Header("Falling")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float fallThreshold = 3f;
    [SerializeField] private float damagePerMeter = 10f;
    private float startY;
    private bool isFalling;
    [SerializeField] private float fallingDeadTime = 5f;
    private float fallingElapsed;

    [Header("Interact")]
    [SerializeField] private float interactHeight = 10f;
    [SerializeField] private Transform thirdPersonInteractPos;
    [SerializeField] private float interactRadius = 1f;
    [SerializeField] private LayerMask interactLayer;
    [HideInInspector] public EnvironmentObject currentInteractObject;

    [Networked, HideInInspector] public bool Grounded { get; set; }
    [Networked, HideInInspector] public Vector3 ChangePos { get; set; }
    [Networked, HideInInspector] public bool IsChangePos { get; set; }

    Collider[] _interactResult = new Collider[5];

    private NetworkButtons prevInputButtons;

    public override void Spawned()
    {
        InitComponents();
    }

    void InitComponents()
    {
        kcc = GetComponent<SimpleKCC>();

        player = GetComponent<Player>();
        movementManager = GetComponent<MovementStateManager>();
        movementManager.ChangeState(MovementState.Idle);

        toolManager = GetComponent<ToolStateManager>();
        toolManager.ChangeState(ToolState.Idle);

        cameraManager = GetComponentInChildren<PlayerCameraManager>();
        if (!HasInputAuthority)
            cameraManager.SetNetworkCamera();

        rigid = GetComponent<Rigidbody>();
        rigid.isKinematic = false;
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<PlayerNetworkInputData>(out var input))
            return;

        // 테스트용
        if (HasInputAuthority && Input.GetKeyDown(KeyCode.H))
            player.RPC_Heal();

        if (HasStateAuthority)
        {
            ChangePosition();

            Falling();
        }

        HandleState(input);

        if (!player.CanMove)
        {
            Gravity();
            return;
        }

        if (input.WasPressed(prevInputButtons, PlayerNetworkInputData.removeInput))
        {
            Debug.Log("AttackPlayer");
            player.TakeDamage(true, 30f);
        }

        HandleMovement(input);

        if (HasInputAuthority)
            TestTryOverlap(input);

        prevInputButtons = input.Buttons;
    }

    private void Falling()
    {
        Vector3 velocity = kcc.RealVelocity;

        // 떨어지기 시작
        if (!isFalling && velocity.y < -0.1f && !Grounded)
        {
            isFalling = true;
            startY = transform.position.y;
        }

        // 떨어지는 중
        if (isFalling && !Grounded)
        {
            fallingElapsed += Runner.DeltaTime;
            if (fallingElapsed > fallingDeadTime)
            {
                fallingElapsed = 0f;
                player.TakeDamage(false, 10000f);
            }
        }

        // 착지
        if (isFalling && Grounded)
        {
            isFalling = false;

            float endY = transform.position.y;
            float fallDistance = startY - endY;

            if (fallDistance > fallThreshold)
            {
                float damage = (fallDistance - fallThreshold) * damagePerMeter;
                player.TakeDamage(false, damage);
                RPC_ShakeCamera();
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ShakeCamera() => cameraManager.ShakeCamera();

    private void HandleState(PlayerNetworkInputData input)
    {
        movementManager.SetInput(input);
        toolManager.SetInput(input);

        if (movementManager.movementStateDict.TryGetValue(movementManager.CurrentMoveState, out var movementState))
            movementState.UpdateState();
        if (toolManager.toolStateDict.TryGetValue(toolManager.CurrentToolState, out var toolState))
            movementState.UpdateState();

        movementManager.SetPrevInputButton(input.Buttons);
        toolManager.SetPrevInputButton(input.Buttons);
    }

    private void ChangePosition()
    {
        if (IsChangePos)
        {
            IsChangePos = false;
            kcc.SetPosition(ChangePos);
        }
    }

    private void Gravity()
    {
        Vector3 velocity = kcc.RealVelocity;
        velocity.y += gravity * Runner.DeltaTime;
        kcc.Move(velocity);
        Grounded = kcc.IsGrounded;
    }

    private void HandleMovement(PlayerNetworkInputData input)
    {
        maxSpeed = player.CanMoving() ? movementManager.currentMoveSpeed : 0f;
        movementManager.MoveValue = input.moveValue;

        if (HasStateAuthority)
            Move(input.moveDir);
        Rotate(input);
    }

    public void Move(Vector3 direction)
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
        Grounded = kcc.IsGrounded;
    }

    private void Rotate(PlayerNetworkInputData input)
    {
        if (input.currentView == ViewType.FirstPerson)
        {
            Quaternion yaw = Quaternion.Euler(0, input.lookValue.x * cameraManager.sensitivity, 0);
            kcc.SetLookRotation(kcc.Transform.rotation * yaw);
        }
        else if (input.currentView == ViewType.ThirdPerson && (input.moveValue.sqrMagnitude > 0.001f || toolManager.IsAiming()))
        {
            if (input.camForward == Vector3.zero)
                return;
            LookForward_ThirdPerson(input);
        }
    }

    public void LookForward_ThirdPerson(PlayerNetworkInputData input)
    {
        Quaternion target = Quaternion.LookRotation(input.camForward);
        kcc.SetLookRotation(Quaternion.Slerp(kcc.Transform.rotation, target, rotationSpeed * Runner.DeltaTime));
    }

    public override void Render()
    {
        kcc.Render();

        movementManager.UpdateMoveAnimation(Runner.DeltaTime);
        toolManager.UpdateMoveAnimation();
    }

    private void TestTryOverlap(PlayerNetworkInputData input)
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
                        player.playerInteractUI.InteractUI(interactable.interactableType);
                        currentInteractObject = interactable;
                        break;
                    }
                }

                // 다른 오브젝트 
                else if (_interactResult[i].TryGetComponent<InteractableObject>(out var interactableObject))
                {
                    player.playerInteractUI.InteractUI(interactableObject.interactableType);
                    player.playerInteractUI.interactableText.text = interactableObject.text;

                    // 설치가능한 오브젝트
                    if (interactableObject.isPlaceable)
                    {
                        if (interactableObject.CanInteract()
                            && input.WasPressed(prevInputButtons, PlayerNetworkInputData.removeInput))
                        {
                            // 제거하고 템창에 넣는 로직 추가하기
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
            player.playerInteractUI.InteractUI();
            currentInteractObject = null;
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

    public void Interact()
    {
        if (currentInteractObject == null)
            return;
        if (currentInteractObject.interactableType == InteractableType.Tree && currentInteractObject.CanInteract())
        {
            int att = player.GetToolAtt("Axe");
            Debug.Log("Player Att : " + att);
            currentInteractObject?.Interact(Object.InputAuthority, att);
        }
        else if (currentInteractObject.interactableType == InteractableType.Stone && currentInteractObject.CanInteract())
        {
            int att = player.GetToolAtt("Pickaxe");
            Debug.Log("Player Att : " + att);
            currentInteractObject?.Interact(Object.InputAuthority, att);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SetPosition(Vector3 position)
    {
        IsChangePos = true;
        ChangePos = position;
    }

    public void SetPosition(Vector3 position)
    {
        IsChangePos = true;
        ChangePos = position;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_FreezePosition(bool freeze) => FreezePosition(freeze);

    public void FreezePosition(bool freeze)
    {
        if (freeze)
        {
            if (HasStateAuthority)
                kcc.ResetVelocity();
            player.CanMove = false;
        }
        else
            player.CanMove = true;
    }
}
