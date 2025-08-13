using Fusion;
using UnityEngine;

namespace YSB_Scripts
{
    public class NetworkTree : EnvironmentObject
    {
        //나중에 tree, stone, pebble 등등 설계가 달라져서 definition이 달라지기 시작하면 이런식으로..
        //지금은 굳이 이렇게 나눌 필요는 없을거 같아서 폐기.
        private TreeDefinition definition;
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
            definition = def as TreeDefinition;
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
                Debug.Log($"Tree[{InstanceId}] Destroyed by player: {player}");

                Die();
            }
        }
    }
}