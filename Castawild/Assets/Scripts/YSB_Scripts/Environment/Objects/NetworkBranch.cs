using UnityEngine;
using Fusion;

namespace YSB_Scripts
{
    public class NetworkBranch : EnvironmentObject
    {
        private BranchDefinition definition;
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
            definition = def as BranchDefinition;
            MaxHP = definition.maxHealth;
            Health = MaxHP;
        }

        public override void Interact(PlayerRef player, int att)
        {
            //this object can be picked up
            if (!IsAlive()) return;

            var playerObj = Runner.GetPlayerObject(player);
            Player _player = playerObj.GetComponent<Player>();
            //InventoryDataManager inventoryData = _player.GetComponent<InventoryDataManager>();
            //inventoryData.AddItem(definition.dropItemID, definition.dropAmount);//아이템 획득
            Die();
        }
    }
}
