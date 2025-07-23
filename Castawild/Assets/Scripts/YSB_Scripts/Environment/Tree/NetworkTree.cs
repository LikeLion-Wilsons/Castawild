using Fusion;
using UnityEngine;

namespace YSB_Scripts
{
    public class NetworkTree : NetworkBehaviour, Test.IInteractable
    {
        public event System.Action<NetworkTree> OnTreeDied;

        //[Networked, OnChangedRender(nameof(OnChangedHealth))]//tree 외형 변화 없으면 굳이 필요 없을지도
        public int Health { get; set; }
        [Networked] private int MaxHP { get; set; }
        [Networked] public int TreeId { get; set; } // TreeId는 스폰 시에 할당
        private TreeDefinition definition;

        [SerializeField] public GameObject visualRoot;

        [Networked] private TickTimer reviveTimer { get; set; }

        public void Init(TreeDefinition def, int treeId)
        {
            if (def == null)
            {
                Debug.LogError("TreeDefinition is null!");
                return;
            }
            definition = def;
            MaxHP = def.maxHealth;
            Health = MaxHP;
            TreeId = treeId;
        }

        // public override void Spawned()
        // {
        //     RefreshVisual();
        // }


        public bool CanInteract() => Health > 0;

        public void Interact(PlayerRef player)
        {
            RPC_RequestDamage(player);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        void RPC_RequestDamage(PlayerRef player)
        {
            if (Health <= 0) return;

            Health -= 10;

            if (Health > 0)
            {
                float ratio = MaxHP > 0 ? (float)Health / MaxHP : 0;
                string log = $"Tree[{TreeId}] Health: {Health}/{MaxHP} ({ratio:P})";
                NetworkLogManager.Instance.Log(log, player);
            }

            if (Health <= 0)
            {
                var playerObj = Runner.GetPlayerObject(player);
                var inven = playerObj.GetComponent<Test.PlayerInventory>();
                inven.AddItem(definition.dropItemID, definition.dropAmount);

                OnTreeDied?.Invoke(this);
            }
        }

        public bool CanRevive() => Health <= 0 && reviveTimer.ExpiredOrNotRunning(Runner);

        public void Revive()
        {
            Health = MaxHP;
            reviveTimer = TickTimer.CreateFromSeconds(Runner, 10f);
        }
    }
}