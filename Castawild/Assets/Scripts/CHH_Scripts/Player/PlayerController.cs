using Fusion;
using Fusion.Addons.SimpleKCC;
using Test;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PlayerController : NetworkBehaviour
{
    public SimpleKCC kcc;
    private Player player;
    private MovementStateManager movementManager;
    private ToolStateManager toolManager;
    private PlayerCameraManager cameraManager;

    [Header("Movement")]
    public float gravity = -20f;
    public float jumpImpulse = 3f;
    public float maxSpeed = 2f;
    public float rotationSpeed = 15f;

    [Header("Interact")]
    [SerializeField] private float interactHeight = 10f;
    [SerializeField] private Transform thirdPersonInteractPos;
    [SerializeField] private float interactRadius = 1f;
    [SerializeField] private LayerMask interactLayer;
    [HideInInspector] public EnvironmentObject currentInteractObject;

    public bool Grounded => kcc.IsGrounded;

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
        movementManager.ChangeState(movementManager.idleState);

        toolManager = GetComponent<ToolStateManager>();
        toolManager.ChangeState(toolManager.idleState);

        cameraManager = GetComponentInChildren<PlayerCameraManager>();
        if (!HasInputAuthority)
            cameraManager.SetNetworkCamera();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput<PlayerNetworkInputData>(out var input))
        {
            // 속도 조절
            maxSpeed = movementManager.CanMove ? movementManager.currentMoveSpeed : 0f;

            movementManager.SetInput(input);
            toolManager.SetInput(input);

            movementManager.currentState.UpdateState();
            toolManager.currentState.UpdateState();

            movementManager.SetPrevInputButton(input.Buttons);
            toolManager.SetPrevInputButton(input.Buttons);

            if (!player.CanAct)
                return;

            TestTryOverlap(input);

            movementManager.MoveValue = input.moveValue;

            Move(input.moveDir);
            Rotate(input);

            prevInputButtons = input.Buttons;
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
        Vector3 horizontalVel = direction * maxSpeed;

        velocity.x = horizontalVel.x;
        velocity.z = horizontalVel.z;

        // 점프
        float jump = 0f;
        if (movementManager.JumpTriggered)
        {
            Debug.Log("KCC.JumpTriggered" + movementManager.JumpTriggered);
            movementManager.JumpTriggered = false;
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
                        player.ChangeCrosshairUI(interactable.interactableType);
                        currentInteractObject = interactable;
                        break;
                    }
                }

                // 다른 오브젝트 
                else if (_interactResult[i].TryGetComponent<InteractableObject>(out var interactableObject))
                {
                    player.interactableUI.alpha = 1f;
                    player.interactableText.text = interactableObject.text;

                    // 설치가능한 오브젝트
                    if (interactableObject.isPlaceable)
                    {
                        player.placeableUI.alpha = 1f;
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
            player.interactableUI.alpha = 0f;
            currentInteractObject = null;
            player.ChangeCrosshairUI();
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
}
