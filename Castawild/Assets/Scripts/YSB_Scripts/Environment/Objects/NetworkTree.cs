using Fusion;
using UnityEngine;

namespace YSB_Scripts
{
    public class NetworkTree : EnvironmentObject
    {
        private TreeDefinition definition;
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

        public override void Interact(PlayerRef player, int att)
        {
            Debug.Log($"Tree[{InstanceId}] Interact with player: {player} for {att} damage");
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
                string log = $"Tree[{InstanceId}] Health: {Health}/{MaxHP} ({ratio:P})";
                NetworkLogManager.Instance.Log(log, player);
            }
            else
            {
                var playerObj = Runner.GetPlayerObject(player);
                Player _player = playerObj.GetComponent<Player>();
                //InventoryDataManager inventoryData = _player.GetComponent<InventoryDataManager>();
                //inventoryData.AddItem(definition.dropItemID, definition.dropAmount);//아이템 획득

                Die();
            }
        }
    }
}