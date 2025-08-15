using Fusion;
using UnityEngine;

namespace YSB_Scripts
{
    public class MineableObject : EnvironmentObject
    {
        private SpawnableDefinition definition;
        public override void Spawned()
        {
            base.Spawned();
            //NetworkObjectVisibilityManager.Instance?.RegisterObject(this); // Spawner로 이동
        }

        public override void Init(SpawnableDefinition def, int instanceId)
        {
            base.Init(def, instanceId);
            if (def == null)
            {
                Debug.LogError("TreeDefinition is null!");
                return;
            }
            definition = def;
            MaxHP = definition.maxHealth;//EnvironmentObject에 가도됨. 나중에 tree 등 세부 설계가 달리지면 이런식으로..
            Health = MaxHP;//EnvironmentObject에 가도됨. 나중에 tree 등 세부 설계가 달리지면 이런식으로..
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

            if (Health <= 0)
            {
                DropItem(player, definition);
                Die();
            }
        }
    }
}