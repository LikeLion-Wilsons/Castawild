using Fusion;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Test
{
    //클래스가 아닌, 구조체.
    public struct InventorySlot : INetworkStruct
    {
        public short id; //	-32,768 to 32,767
        public byte count; // 0 to 255
    }

    public class PlayerInventory : NetworkBehaviour
    {
        [Networked, Capacity(30)] public NetworkLinkedList<InventorySlot> Slots => default;

        public void AddItem(int _id, int _count)
        {
            short id = ToShortChecked(_id);
            byte count = ToByteChecked(_count);

            bool added = false;
            for (int i = 0; i < Slots.Count; i++)
            {
                var slot = Slots.Get(i);
                if (slot.id == id)
                {
                    slot.count += count;
                    Slots.Set(i, slot);
                    added = true;
                    break;
                }
            }

            if (added == false)
            {
                Slots.Add(new InventorySlot() { id = id, count = count });
            }

            ShowLog();
        }

        public int GetItem(int id)
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].id == id)
                {
                    return Slots[i].count;
                }
            }

            return 0;
        }

        public void RemoveItem(int _id, int _count)
        {
            short id = ToShortChecked(_id);
            byte count = ToByteChecked(_count);
            
            for (int i = 0; i < Slots.Count; i++)
            {
                var slot = Slots.Get(i);
                if (slot.id == id)
                {
                    if (slot.count - count <= 0)
                    {
                        Slots.Remove(slot);
                    }
                    else
                    {
                        slot.count -= count;
                        Slots.Set(i, slot);
                    }

                    break;
                }
            }

            ShowLog();
        }


        private StringBuilder sb = new StringBuilder();

        public void ShowLog()
        {
            sb.Clear();
            for (int i = 0; i < Slots.Count; i++)
            {
                var slot = Slots.Get(i);
                sb.AppendLine($"id:[{slot.id}],count:[{slot.count}]");
            }

            TestLog(sb.ToString(), Object.InputAuthority);
        }


        #region TEST_LOG

        //why? 씬에는 플레이어가 여러개있으므로 로그가 여러개 뜸.
        //로그를 요청한 클라에서만 로그찍히게 함.

        void TestLog(string message, PlayerRef player)
        {
            if (Runner.LocalPlayer == player)
            {
                Debug.Log($"[Self#{player.PlayerId}]: {message}");
            }
            else
            {
                RPC_Request(message, player);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        void RPC_Request(string message, PlayerRef player)
        {
            RPC_BroadCast(message, player);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        void RPC_BroadCast(string message, PlayerRef player)
        {
            if (Runner.LocalPlayer == player)
            {
                Debug.Log($"[SelfRPC#{player.PlayerId}]: {message}");
            }
        }

        #endregion
        

        #region Util
        public short ToShortChecked(int value)
        {
            try { return checked((short)value); }
            catch (OverflowException)
            {
                Debug.LogWarning($"short 범위를 벗어났습니다: {value}");
                return (short)Mathf.Clamp(value, short.MinValue, short.MaxValue);
            }
        }

        public byte ToByteChecked(int value)
        {
            try { return checked((byte)value); }
            catch (OverflowException)
            {
                Debug.LogWarning($"byte 범위를 벗어났습니다: {value}");
                return (byte)Mathf.Clamp(value, byte.MinValue, byte.MaxValue);
            }
        }
        #endregion
    }
}