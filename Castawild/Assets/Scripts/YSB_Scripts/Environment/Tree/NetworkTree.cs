using Fusion;
using UnityEngine;

namespace YSB_Scripts
{
    public class NetworkTree : NetworkBehaviour, Test.IInteractable
    {
        public event System.Action<NetworkTree> OnTreeDied;

        [Networked, OnChangedRender(nameof(OnChangedHealth))]//tree 외형 변화 없으면 굳이 필요 없을지도
        public int Health { get; set; }
        [Networked] private int MaxHP { get; set; }
        [Networked] public int TreeId { get; set; } // TreeId는 스폰 시에 할당
        private TreeDefinition definition;

        [SerializeField] public GameObject visualRoot;

        [Networked] private TickTimer reviveTimer { get; set; }

        // --- 추가 ---
        // Render()에서 비주얼을 딱 한 번만 초기화하기 위한 플래그
        private bool _visualsInitialized = false;

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

        // --- 수정 ---
        // Spawned()는 데이터 초기화에만 집중하고, 시각적 처리는 Render()로 넘깁니다.
        public override void Spawned()
        {
            // RefreshVisual() 호출을 여기서 제거합니다.
        }

        // --- 추가 ---
        // Render()는 네트워크 상태가 객체의 Transform에 완전히 적용된 후에 호출됩니다.
        public override void Render()
        {
            if (!_visualsInitialized)
            {
                RefreshVisual();
                _visualsInitialized = true;
            }
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

        void OnChangedHealth()//tree 외형 변화 없으면 굳이 필요 없을지도
        {
            RefreshVisual();
        }

        void RefreshVisual()
        {
            // visualRoot가 할당되지 않았으면 아무것도 하지 않습니다.
            if (visualRoot == null) return;

            bool isAlive = Health > 0;
            visualRoot.SetActive(isAlive);

            if (isAlive)
            {
                float ratio = MaxHP > 0 ? (float)Health / MaxHP : 0;
                // Debug.Log($"Tree Health: {Health}/{MaxHP} ({ratio:P})");
            }
        }
    }
}