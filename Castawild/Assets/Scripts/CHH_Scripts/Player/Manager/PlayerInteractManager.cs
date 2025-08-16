using Fusion;
using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerInteractManager : NetworkBehaviour
{
    private Player player;
    private PlayerMoveManager moveManager;
    private PlayerToolManager toolManager;
    [SerializeField] private OptionUI optionUI;
    private PlayerInteractUI playerInteractUI;
    private MovementStateManager movementManager;
    private ToolStateManager toolStateManager;
    private PlayerFlagManager flagManager;

    [Header("Interact")]
    [SerializeField] private float interactHeight = 10f;
    [SerializeField] private Transform thirdPersonInteractPos;
    [SerializeField] private float interactRadius = 1f;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private float kneelY = 6.8f;
    [HideInInspector] public EnvironmentObject Client_currentInteractObject;

    Collider[] _interactResult = new Collider[5];

    private NetworkButtons prevInputButtons;
    public event Action<int> Hit;

    public override void Spawned()
    {
        InitComponents();
        optionUI.SetSessionName(Runner.SessionInfo.Name);
    }

    private void InitComponents()
    {
        player = GetComponent<Player>();
        moveManager = GetComponent<PlayerMoveManager>();
        toolManager = GetComponent<PlayerToolManager>();
        playerInteractUI = GetComponentInChildren<PlayerInteractUI>();

        movementManager = GetComponent<MovementStateManager>();
        movementManager.Host_ChangeState(MovementState.Idle);

        toolStateManager = GetComponent<ToolStateManager>();
        toolStateManager.Host_ChangeState(ToolState.Idle);

        flagManager = GetComponent<PlayerFlagManager>();
    }

    public override void FixedUpdateNetwork()
    {
        if (flagManager.IsDead)
            return;

        if (!GetInput<PlayerNetworkInputData>(out var input))
            return;

        if (!moveManager.CanMove)
            return;

        if (HasInputAuthority && !player.inventory.canvasHolder.AnyUIOpen() && !optionUI.gameObject.activeSelf)
            Client_TestTryOverlap(input);

        prevInputButtons = input.Buttons;
    }

    public override void Render()
    {
        movementManager.All_UpdateMoveAnimation(Runner.DeltaTime);
        toolStateManager.All_UpdateMoveAnimation();
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
                        if (Client_currentInteractObject == null)
                            Debug.Log("currentInteractObject is null");

                        if (interactable.interactableType == InteractableType.Gatherable)
                        {
                            playerInteractUI.SetInteractText("줍기");
                            if (input.WasPressed(prevInputButtons, PlayerNetworkInputData.interactInput))
                            {
                                movementManager.RPC_RequestChangeGatherState(Object.InputAuthority);

                                float targetTopY = interact.bounds.max.y;
                                if (targetTopY - transform.position.y <= kneelY)
                                    movementManager.RPC_RequestSetKneel(false);
                                else
                                    movementManager.RPC_RequestSetKneel(true);
                            }
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

    public void Client_Gather()
    {
        if (Client_currentInteractObject == null)
            Debug.Log("Gather - currentInteractObject is null");

        Client_currentInteractObject?.Interact(Object.InputAuthority, 999);
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
        if (Client_currentInteractObject == null || !HasInputAuthority
            || Client_currentInteractObject.interactableType == InteractableType.Gatherable)
            return;

        int att = 0;
        if (Client_currentInteractObject.interactableType == InteractableType.Tree && Client_currentInteractObject.CanInteract())
        {
            SoundManager.Instance.PlayGlobalSound3D(Sound.Player_Attack, player.soundPosition.position);
            att = toolManager.All_GetToolAtt("Axe");
            Client_currentInteractObject?.Interact(Object.InputAuthority, att);
            toolStateManager.RPC_RequestDecreaseToolDuration(true);
        }
        else if (Client_currentInteractObject.interactableType == InteractableType.Stone && Client_currentInteractObject.CanInteract())
        {
            SoundManager.Instance.PlayGlobalSound3D(Sound.Player_Attack, player.soundPosition.position);
            att = toolManager.All_GetToolAtt("Pickaxe");
            Client_currentInteractObject?.Interact(Object.InputAuthority, att);
            toolStateManager.RPC_RequestDecreaseToolDuration(true);
        }

        if (att != 0)
            Hit?.Invoke(att);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ApplyHitInvoke(int dmg) => Hit?.Invoke(dmg);

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_DespawnObject(NetworkObject despawnObject) => Runner.Despawn(despawnObject);

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ApplySetWakeUpUI() => playerInteractUI.SetWakeUpUI();
}
