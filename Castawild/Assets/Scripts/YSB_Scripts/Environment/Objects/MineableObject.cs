using Fusion;
using UnityEngine;

namespace YSB_Scripts
{
    public class MineableObject : EnvironmentObject
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
