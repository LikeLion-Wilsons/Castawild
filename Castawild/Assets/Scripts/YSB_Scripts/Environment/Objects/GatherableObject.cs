using Fusion;
using UnityEngine;

namespace YSB_Scripts
{
    public class GatherableObject : EnvironmentObject
    {
        private SpawnableDefinition definition;

        public override void Init(SpawnableDefinition def, int instanceId)
        {
            base.Init(def, instanceId);

            if (def == null)
            {

                return;
            }

            definition = def;
            MaxHP = def.maxHealth;
            Health = MaxHP;
        }

        public override void Interact(PlayerRef player, int att)
        {
            if (!IsAlive()) return;

            RPC_RequestGather(player);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestGather(PlayerRef player)
        {
            if (!IsAlive()) return;

            Health = 0; 
            DropItem(player, definition); 
            Die(); 
        }
    }
}
