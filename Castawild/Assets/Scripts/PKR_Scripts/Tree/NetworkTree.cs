using Fusion;
using UnityEngine;

namespace Test
{
    public interface IInteractable
    {
        void Interact(PlayerRef player);
    }

    public class NetworkTree : NetworkBehaviour, IInteractable
    {
        public NetworkHealth health;
        private PlayerRef lastAttacker;
        public GameObject visualRoot;
        [Networked] private TickTimer _respawnTimer { get; set; }

        public void Interact(PlayerRef player)
        {
            if (!health.IsAlive()) return;
            if (!HasStateAuthority) return;

            if(Runner.LocalPlayer == player)
                Debug.Log("Take Damage 10!");
            lastAttacker = player;
            health.TakeDamage(10);
            if (health.IsAlive() == false)
            {
                Die();
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (health.IsAlive()) return;
            bool isActive = _respawnTimer.ExpiredOrNotRunning(Runner);
            if (isActive)
            {
                Revive();
            }
        }

        void Die()
        {
            visualRoot.SetActive(false);
            _respawnTimer = TickTimer.CreateFromSeconds(Runner, 3f);


            var playerObj = Runner.GetPlayerObject(lastAttacker);
            playerObj.GetComponent<PlayerInventory>().materialWood += 10;

            if (lastAttacker == Runner.LocalPlayer)
            {
                Debug.Log("나무를 얻었다!");
            }
        }

        void Revive()
        {
            visualRoot.SetActive(true);
            health.Revive();
        }
    }
}