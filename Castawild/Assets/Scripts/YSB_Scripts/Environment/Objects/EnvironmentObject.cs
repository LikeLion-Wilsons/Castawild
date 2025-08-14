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

    [Networked, /*OnChangedRender(nameof(OnChangedHealth))*/] protected int Health { get; set; }
    [Networked] protected int MaxHP { get; set; }
    [Networked] protected TickTimer ReviveTimer { get; set; }
    private float reviveTime;
    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && CanRevive())
        {
            Revive();
        }
    }

    public virtual void Init(SpawnableDefinition def, int instanceId)
    {
        col = GetComponent<Collider>();
        visualRootController = visualRoot.GetComponent<VisualRootController>();
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
        col.enabled = true;
        visualRootController.SetVisible(true);
    }

    // void OnChangedHealth()
    // {
    //     Debug.Log($"{Object.name}[{InstanceId}] Health changed: {Health}/{MaxHP}");
    // }

    protected void Die()
    {
        Debug.Log($"{Object.name}[{InstanceId}] Destroyed");
        //OnDestroyed?.Invoke(this);
        ReviveTimer = TickTimer.CreateFromSeconds(Runner, reviveTime);
        col.enabled = false;
        visualRootController.SetVisible(false);
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
