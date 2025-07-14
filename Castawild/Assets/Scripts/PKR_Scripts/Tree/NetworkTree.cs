using Fusion;
using UnityEngine;

namespace Test
{
    public interface IInteractable
    {
        void Interact(PlayerRef playerRef);
    }

    public class NetworkTree : NetworkBehaviour, IInteractable
    {
        [Networked, OnChangedRender(nameof(OnChangedHealth))] public int Health { get; set; }

        [SerializeField] private GameObject tree3;
        [SerializeField] private GameObject tree2;
        [SerializeField] private GameObject tree1;

        public void Init(int initHp)
        {
            Health = initHp;
        }

        public override void Spawned()
        {
            Refresh();
        }

        public void Interact(PlayerRef player)
        {
            RPC_Request(player);
        }

        //클라->서버
        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        void RPC_Request(PlayerRef player)
        {
            Health -= 10;
            if (Health <= 0)
            {
                //막타 플레이어에게 아이템지급.
                var playerObj = Runner.GetPlayerObject(player);
                var inven = playerObj.GetComponent<PlayerInventory>();
                inven.AddItem("wood", 1);
                Runner.Despawn(Object);
            }
        }

        void OnChangedHealth()
        {
            Debug.Log($"나무체력: {Health}");
            Refresh();
        }

        void Refresh()
        {
            tree3.SetActive(Health >= 100);
            tree2.SetActive(Health >= 50);
            tree1.SetActive(Health >= 10);
        }
    }
}