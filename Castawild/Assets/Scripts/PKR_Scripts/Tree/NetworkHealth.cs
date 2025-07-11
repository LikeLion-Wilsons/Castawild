using Fusion;

namespace Test
{
    public class NetworkHealth : NetworkBehaviour
    {
        [Networked] public int CurrentHP { get; set; }
        private int MaxHP;

        public void Init(int maxHp)
        {
            MaxHP = CurrentHP = maxHp;
        }

        public void TakeDamage(int amount)
        {
            if (!Runner.IsServer) return;

            CurrentHP -= amount;
        }


        public bool IsAlive()
        {
            return CurrentHP > 0;
        }

        public void Revive()
        {
            CurrentHP = MaxHP;
        }
    }
}