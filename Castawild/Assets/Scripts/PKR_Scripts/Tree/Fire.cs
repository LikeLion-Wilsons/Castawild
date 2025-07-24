using Fusion;
using UnityEngine;

namespace Test
{
    public class Fire : NetworkBehaviour, IInteractable
    {
        [SerializeField] private ParticleSystem ps;
        [Networked] private TickTimer timer { get; set; }

        public bool CanInteract()
        {
            return timer.ExpiredOrNotRunning(Runner);
        }

        public void Interact(PlayerRef playerRef)
        {
            RPC_Request(playerRef);
        }

        //클라->서버
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        void RPC_Request(PlayerRef player)
        {
            var playerObj = Runner.GetPlayerObject(player);
            var inven = playerObj.GetComponent<PlayerInventory>();
            if (inven.GetItem(1000) > 0)
            {
                inven.RemoveItem(1000, 1);
                timer = TickTimer.CreateFromSeconds(Runner, ps.totalTime);
                RPC_Broadcast();
            }
        }

        //서버-> 모든클라
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        void RPC_Broadcast()
        {
            ps.Play();
        }
    }
}