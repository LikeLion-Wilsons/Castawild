using Fusion;
using UnityEngine;

namespace YSB_Scripts
{
    public class GatherableObject : EnvironmentObject
    {
        protected SpawnableDefinition definition;
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
            //this object can be picked up
            if (!IsAlive()) return;

            Health = 0;

            var playerObj = Runner.GetPlayerObject(player);
            Player _player = playerObj.GetComponent<Player>();
            InventoryDataManager inventoryData = _player.inventory;
            inventoryData.AddItem(definition.dropItemID, definition.dropAmount);//아이템 획득
            Debug.Log($"{Object.name}[{InstanceId}] Destroyed by player: {player}");

            Die();
        }
    }
}