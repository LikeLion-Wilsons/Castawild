    using Fusion;
    using UnityEngine;

namespace YSB_Scripts
{
    public class NetworkTree : NetworkBehaviour, Test.IInteractable
    {
        public event System.Action<NetworkTree> OnTreeDied;

        [Networked, OnChangedRender(nameof(OnChangedHealth))]
        public int Health { get; set; }
        private int maxHP;
        private TreeDefinition definition;
        [SerializeField] public GameObject visualRoot;
        [Networked] private TickTimer reviveTimer { get; set; }


        // [SerializeField] private GameObject treeStage1;
        // [SerializeField] private GameObject treeStage2;
        // [SerializeField] private GameObject treeStage3;

        public void Init(TreeDefinition def)
        {
            definition = def;
            maxHP = def.maxHealth;
            Health = maxHP;
        }

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

            if (Health <= 0)
            {
                // 드랍 아이템 지급
                var playerObj = Runner.GetPlayerObject(player);
                var inven = playerObj.GetComponent<Test.PlayerInventory>();
                inven.AddItem(definition.dropItemID, definition.dropAmount);

                // 이펙트 / 사운드
                // if (!string.IsNullOrEmpty(definition.destroySFX))
                //     SoundManager.Instance?.PlaySound(definition.destroySFX, transform.position);

                // if (!string.IsNullOrEmpty(definition.destroyVFX))
                //     EffectManager.Instance?.PlayEffect(definition.destroyVFX, transform.position);

                //reviveTimer = TickTimer.CreateFromSeconds(Runner, 10f);

                visualRoot.SetActive(false); // 비주얼 숨기기
                OnTreeDied?.Invoke(this);  
            }
        }

        public bool CanRevive() => Health <= 0 && reviveTimer.ExpiredOrNotRunning(Runner);

        public void Revive()
        {
            Health = maxHP;
        }

        void OnChangedHealth()
        {
            RefreshVisual();
        }

        void RefreshVisual()
        {
            float ratio = (float)Health / maxHP;
            Debug.Log($"Tree Health: {Health}/{maxHP} ({ratio:P})");
            // treeStage1.SetActive(ratio >= 0.7f);
            // treeStage2.SetActive(ratio >= 0.4f && ratio < 0.7f);
            // treeStage3.SetActive(ratio > 0 && ratio < 0.4f);
        }
    }
}
