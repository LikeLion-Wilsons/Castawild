using Fusion;
using UnityEngine;

public class NetworkCampFire : NetworkBehaviour
{
    [Networked] public Item cookPotItem { get; set; }
    [Networked] public Item resultItem { get; set; }
    [Networked] public bool isFire { get; set; } = true;//나중에 바꾸기
    Player player;
    Campfire campfire;
    public InventoryDataManager inventoryData;
    private double nextCookTime;
    private bool isCooking;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Item item = new Item
            {
                itemID = -1,
                count = 0,
                durability = 1
            };
            cookPotItem = item;
            resultItem = item;
        }

        campfire = GetComponent<Campfire>();
    }

    public override void FixedUpdateNetwork()
    {
        //if (!Object.HasStateAuthority) return; // 호스트만 로직 수행


        //불이 꺼져있으면 타이머 리셋
        if (!isFire)
        {
            isCooking = false;
            return;
        }

        // 아이템이 있을 때만 타이머 작동
        if (inventoryData.itemList[45].itemID != -1 && inventoryData.itemList[45].count > 0)
        {
            if (!isCooking)
            {
                // 처음 조건 만족 시 타이머 시작
                nextCookTime = Runner.SimulationTime + 10.0;
                isCooking = true;
            }

            // 10초가 지났으면 Cook 실행 후 다음 시간 예약
            if (Runner.SimulationTime >= nextCookTime)
            {
                Cook();
                nextCookTime = Runner.SimulationTime + 10.0; // 다음 10초 예약
            }
            Debug.Log($"남은 시간: {nextCookTime - Runner.SimulationTime:F1}초");
        }
        else
        {
            // 조건 불만족 시 타이머 초기화
            isCooking = false;
        }
    }
    private void Cook()
    {
        Debug.Log("요리 완성!");
        // 실제 요리 처리 로직
        Item item = new Item
        {
            itemID = inventoryData.itemList[45].itemID,
            count = inventoryData.itemList[45].count - 1
        };
        inventoryData.RPC_SetItem(45, item);
        Item result = new Item
        {
            itemID = 7,//구운 고기
            count = inventoryData.itemList[46].count + 1
        };
        inventoryData.RPC_SetItem(46, result);
        inventoryData.RPC_UpdateInventoryUI();
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetCookPotItem(Item item)
    {
        cookPotItem = item;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetResultItem(Item item)
    {
        resultItem = item;
    }
}