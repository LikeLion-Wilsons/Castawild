using Fusion;

namespace Test
{
    public class PlayerInventory : NetworkBehaviour
    {
        [Networked] public int materialWood { get; set; }

    }
}