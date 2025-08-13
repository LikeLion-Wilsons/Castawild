using Fusion;
using UnityEngine;
using System;
using UnityEditor;

[RequireComponent(typeof(NetworkObject),typeof(NetworkTransform))]
public abstract class EnvironmentObject : NetworkBehaviour, ISpawnable, INetworkVisibilityObject, YSB_Scripts.IInteractable, IRevivable
{
    public event Action<INetworkVisibilityObject> OnDestroyed;
    public InteractableType interactableType;
    public int InstanceId { get; set; }
    public GameObject GameObject { get { return gameObject; } }
    public GameObject VisualRoot { get { return visualRoot; } }
    public Collider Collider { get { return cachedCollider; } }
    private Collider cachedCollider;
    [SerializeField] private GameObject visualRoot;

    [Networked] protected int Health { get; set; }
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
        cachedCollider = GetComponent<Collider>();
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
    }

    protected void Die()
    {
        OnDestroyed?.Invoke(this);
        ReviveTimer = TickTimer.CreateFromSeconds(Runner, reviveTime);
    }
}
