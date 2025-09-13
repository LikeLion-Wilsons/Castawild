using Fusion;
using UnityEngine;
using System;

[RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
public abstract class EnvironmentObject : NetworkBehaviour, ISpawnable, YSB_Scripts.IInteractable, IRevivable
{
    public InteractableType interactableType;
    public int InstanceId { get; set; }

    private Collider col;
    [SerializeField] private GameObject visualRoot;
    private VisualRootController visualRootController;

    [Networked, OnChangedRender(nameof(OnChangedHealth))] protected int Health { get; set; }
    [Networked] protected int MaxHP { get; set; }
    [Networked] protected TickTimer ReviveTimer { get; set; }

    private float reviveTime;

    [SerializeField] private float cullDistance = 500f;
    private Transform playerCamera;

    private float cullCheckInterval = 0.3f;
    private float lastCullCheck;

    public override void Spawned()
    {
        base.Spawned();

        if (visualRoot != null)
            visualRootController = visualRoot.GetComponent<VisualRootController>();

        col = GetComponent<Collider>();

        // 멀티플레이 고려 → 로컬 플레이어 카메라 할당
        if (Object.HasInputAuthority)
            playerCamera = Camera.main?.transform;

        UpdateCulling(true);
    }

    private void Update()
    {
        if (!IsAlive() || playerCamera == null) return;

        if (Time.time - lastCullCheck >= cullCheckInterval)
        {
            UpdateCulling();
            lastCullCheck = Time.time;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && CanRevive())
            Revive();
    }

    private void UpdateCulling(bool force = false)
    {
        if (playerCamera == null || visualRootController == null) return;

        float distance = Vector3.SqrMagnitude(transform.position - playerCamera.position);
        bool shouldBeVisible = distance <= cullDistance * cullDistance;

        if (force || visualRootController.IsVisible != shouldBeVisible)
            visualRootController.SetVisible(shouldBeVisible);
    }

    public virtual void Init(SpawnableDefinition def, int instanceId)
    {
        reviveTime = def.reviveTime;
        InstanceId = instanceId;
        MaxHP = def.maxHealth;
        Health = MaxHP;
    }

    public virtual bool IsAlive() => Health > 0;
    public virtual bool CanBeVisible() => IsAlive();
    public virtual bool CanInteract() => IsAlive();
    public virtual void Interact(PlayerRef playerRef, int att) { }
    public virtual bool CanRevive() => !IsAlive() && ReviveTimer.ExpiredOrNotRunning(Runner);

    public virtual void Revive()
    {
        Health = MaxHP;
        RPC_UpdateVisualState(true);
    }

    void OnChangedHealth() => SyncVisualState();

    private void SyncVisualState()
    {
        bool alive = IsAlive();
        if (col != null)
            col.enabled = alive;

        if (visualRootController != null)
            visualRootController.SetVisible(alive);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateVisualState(bool isAlive)
    {
        if (visualRootController == null && visualRoot != null)
            visualRootController = visualRoot.GetComponent<VisualRootController>();

        if (visualRootController != null)
        {
            visualRootController.SetVisible(isAlive);
            if (col != null)
                col.enabled = isAlive;
        }
    }

    protected void Die()
    {
        Debug.Log($"{Object.name}[{InstanceId}] Destroyed");
        ReviveTimer = TickTimer.CreateFromSeconds(Runner, reviveTime);
        RPC_UpdateVisualState(false);
    }

    protected void DropItem(PlayerRef player, SpawnableDefinition definition)
    {
        Debug.Log($"{Object.name}[{InstanceId}] Dropping item: {definition.dropItem.itemID}, Amount: {definition.dropAmount}");
        var playerObj = Runner.GetPlayerObject(player);
        Player _player = playerObj.GetComponent<Player>();
        InventoryDataManager inventoryData = _player.inventory;
        inventoryData.AddItem(definition.dropItem.itemID, definition.dropAmount);
    }
}

