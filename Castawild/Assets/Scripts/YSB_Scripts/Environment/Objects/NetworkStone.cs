using UnityEngine;
using Fusion;

namespace YSB_Scripts
{
    public class NetworkStone : EnvironmentObject
    {
        private StoneDefinition definition;
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
                Debug.LogError("StoneDefinition is null!");
                return;
            }
            definition = def as StoneDefinition;
            MaxHP = definition.maxHealth;
            Health = MaxHP;
        }

        public override void Interact(PlayerRef player, int att)
        {
            if (!IsAlive()) return;

            RPC_RequestDamage(player, att);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestDamage(PlayerRef player, int att)
        {
            if (!IsAlive()) return;

            Health -= att;

            if (Health > 0)
            {
                float ratio = MaxHP > 0 ? (float)Health / MaxHP : 0;
                string log = $"Stone[{InstanceId}] Health: {Health}/{MaxHP} ({ratio:P})";
                NetworkLogManager.Instance.Log(log, player);
            }
            else
            {
                var playerObj = Runner.GetPlayerObject(player);
                var inven = playerObj.GetComponent<Test.PlayerInventory>();
                inven.AddItem(definition.dropItemID, definition.dropAmount);

                Die();
            }
        }
    }
}
