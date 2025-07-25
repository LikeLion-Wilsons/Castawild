using Fusion;
using UnityEngine;

namespace YSB_Scripts
{
    public interface IInteractable//나중에 분리
    {
        bool CanInteract();
        void Interact(PlayerRef player);
    }
    public class NetworkTree : EnvironmentObject
    {
        [SerializeField] private GameObject visualRoot;
        private TreeDefinition definition;

        public override GameObject VisualRoot => visualRoot;

        public override void Spawned()
        {
            base.Spawned();
            NetworkObjectVisibilityManager.Instance?.RegisterObject(this);
        }

        public override void Init(SpawnableDefinition def, int instanceId)
        {
            base.Init(def, instanceId);
            if (def == null)
            {
                Debug.LogError("TreeDefinition is null!");
                return;
            }
            definition = def as TreeDefinition;
            MaxHP = definition.maxHealth;
            Health = MaxHP;
        }

        public override void Interact(PlayerRef player)
        {
            if (!IsAlive()) return;

            RPC_RequestDamage(player);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestDamage(PlayerRef player)
        {
            if (!IsAlive()) return;

            Health -= 10;

            if (Health > 0)
            {
                float ratio = MaxHP > 0 ? (float)Health / MaxHP : 0;
                string log = $"Tree[{InstanceId}] Health: {Health}/{MaxHP} ({ratio:P})";
                NetworkLogManager.Instance.Log(log, player);
            }
            else
            {
                var playerObj = Runner.GetPlayerObject(player);
                var inven = playerObj.GetComponent<Test.PlayerInventory>();
                inven.AddItem(definition.dropItemID, definition.dropAmount);

                Die();

                ReviveTimer = TickTimer.CreateFromSeconds(Runner, 10f);
            }
        }
    }
}
