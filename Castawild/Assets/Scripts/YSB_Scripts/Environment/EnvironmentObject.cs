using Fusion;
using UnityEngine;
public abstract class EnvironmentObject : NetworkBehaviour, ISpawnable, INetworkVisibilityObject, YSB_Scripts.IInteractable, IRevivable
    {
        public int InstanceId { get; set; }
        public abstract GameObject VisualRoot { get; }

        [Networked] protected int Health { get; set; }
        [Networked] protected int MaxHP { get; set; }
        [Networked] protected TickTimer ReviveTimer { get; set; }

        public event System.Action<NetworkBehaviour> OnDied;

        public virtual void Init(SpawnableDefinition def, int instanceId)
        {
            InstanceId = instanceId;
            // 공통 초기화 (서브 클래스에서 추가 처리 가능)
        }

        public virtual bool IsAlive() => Health > 0;

        // INetworkVisibilityObject
        public virtual bool CanBeVisible() => IsAlive();
        public virtual NetworkObject GetNetworkObject() => Object;

        // IInteractable
        public virtual bool CanInteract() => IsAlive();

        public virtual void Interact(PlayerRef player) { }

        // IRevivable
        public virtual bool CanRevive() => !IsAlive() && ReviveTimer.ExpiredOrNotRunning(Runner);

        public virtual void Revive()
        {
            Health = MaxHP;
            ReviveTimer = TickTimer.CreateFromSeconds(Runner, 10f);
        }

        protected void Die()
        {
            OnDied?.Invoke(this);
        }
    }
