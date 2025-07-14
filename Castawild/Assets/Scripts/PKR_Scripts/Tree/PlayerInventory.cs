using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class PlayerInventory : NetworkBehaviour
    {
        private Dictionary<string, int> inventory = new Dictionary<string, int>();

        public void AddItem(string name, int count)
        {
            if (inventory.ContainsKey(name))
            {
                inventory[name] += count;
            }
            else
            {
                inventory.Add(name, count);
            }

            if (Runner.LocalPlayer == Object.InputAuthority)
            {
                Debug.Log($"{name}아이템을, {count} 획득했습니다.  total:[{inventory[name]}]");
            }
        }
    }
}