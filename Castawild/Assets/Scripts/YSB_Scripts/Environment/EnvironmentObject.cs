using Fusion;
using UnityEngine;
public abstract class EnvironmentObject : NetworkBehaviour, ISpawnable, INetworkVisibilityObject, YSB_Scripts.IInteractable, IRevivable
{
    public InteractableType interactableType;
    public int InstanceId { get; set; }
    public GameObject VisualRoot { get { return visualRoot; } }
    [SerializeField] private GameObject visualRoot;

    [Networked] protected int Health { get; set; } 
    [Networked] protected int MaxHP { get; set; } 
    [Networked] protected TickTimer ReviveTimer { get; set; }
    [SerializeField] private float reviveTime = 10f;

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && CanRevive())
        {
            Revive();
        }
    }

    public virtual void Init(SpawnableDefinition def, int instanceId)
    {
        InstanceId = instanceId;
    }

    public virtual bool IsAlive() => Health > 0;

    // INetworkVisibilityObject
    public virtual bool CanBeVisible() => IsAlive();
    public virtual NetworkObject GetNetworkObject() => Object;

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
        ReviveTimer = TickTimer.CreateFromSeconds(Runner, reviveTime);
    }
}
