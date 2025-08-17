using Fusion;
using UnityEngine;
using System;
using UnityEditor;

[RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
public abstract class EnvironmentObject : NetworkBehaviour, ISpawnable, YSB_Scripts.IInteractable, IRevivable
{
    //public event Action<INetworkVisibilityObject> OnDestroyed;
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
    public override void Spawned()
    {
        base.Spawned();

        if (visualRootController == null && visualRoot != null)
            visualRootController = visualRoot.GetComponent<VisualRootController>();

        col = GetComponent<Collider>();
        playerCamera = Camera.main.transform;
        UpdateCulling();
    }

    private void Update()
    {
        if (!IsAlive()) return;

        UpdateCulling();
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && CanRevive())
        {
            Revive();
        }
    }

    private void UpdateCulling()
    {
        if (playerCamera == null || visualRootController == null) return;

        float distance = Vector3.Distance(transform.position, playerCamera.position);

        visualRootController.SetVisible(distance <= cullDistance);
    }

    public virtual void Init(SpawnableDefinition def, int instanceId)
    {
        reviveTime = def.reviveTime;
        InstanceId = instanceId;
    }

    public virtual bool IsAlive() => Health > 0;

    // INetworkVisibilityObject
    public virtual bool CanBeVisible() => IsAlive();

    // IInteractable
    public virtual bool CanInteract() => IsAlive();

    public virtual void Interact(PlayerRef playerRef, int att) { }

    // IRevivable
    public virtual bool CanRevive() => !IsAlive() && ReviveTimer.ExpiredOrNotRunning(Runner);

    public virtual void Revive()
    {
        Health = MaxHP;
        RPC_UpdateVisualState(true);
    }

    void OnChangedHealth()//후입자 sync
    {
        SyncVisualState();
    }

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
            col.enabled = isAlive;
        }
    }

    protected void Die()
    {
        Debug.Log($"{Object.name}[{InstanceId}] Destroyed");
        //OnDestroyed?.Invoke(this);
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
