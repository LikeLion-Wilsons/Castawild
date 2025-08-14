using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NetworkCampFire : NetworkBehaviour
{
    [Header("슬롯")]
    [Networked] public Item cookPotItem { get; set; }
    [Networked] public Item resultItem { get; set; }

    [Header("불")]
    public bool isFire => fireTime > 0;
    [SerializeField] GameObject fireVFX;

    [Header("시간")]
    [Networked] public float fireTime { get; set; }
    [Networked] private TickTimer fireDecreaseTimer { get; set; }
    [Networked] public TickTimer nextCookTime { get; set; }

    public bool isCooking;

    public Player player;
    Campfire campfire;
    public InventoryDataManager inventoryData;




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
    private double nextFireDecreaseTime;

    public override void FixedUpdateNetwork()
    {
        // 불이 켜져 있을 때 fireTime 감소 처리

        if (fireTime > 0 && Object.HasStateAuthority)
        {
            RPC_SetFireVFX(true);
            
            //만료되었거나, 시작되지않았으면.
            if (fireDecreaseTimer.ExpiredOrNotRunning(Runner))
            { 
                fireDecreaseTimer = TickTimer.CreateFromSeconds(Runner, 1f);
                
                //여기가 1초마다 실행됨.
                fireTime--;
                int min = (int)fireTime / 60;
                int sec = (int)fireTime % 60;
                RPC_SetTimerText(min, sec);
                Debug.Log("fireTime : " + fireTime);
                if (fireTime <= 0)
                {
                    Debug.Log("모닥불 꺼짐...?");
                    isCooking = false; // 불 꺼졌으니 요리 중지
                    RPC_SetFireVFX(false);
                    return; // 불 꺼졌으면 아래 요리 로직은 실행 안 함
                }
            }
        }
        else
        {
            isCooking = false;
            if (Object.HasStateAuthority)
                RPC_SetFireVFX(false);
            return;
        }
        if (inventoryData == null) return;
        // 아이템이 있을 때만 타이머 작동
        if (inventoryData.itemList[45].itemID != -1 || cookPotItem.itemID != -1)
        {
            if (!isCooking)
            {
                nextCookTime = TickTimer.CreateFromSeconds(Runner, 10f);
                isCooking = true;
            }

            // 10초가 지났으면 Cook 실행 후 다음 시간 예약
            if (nextCookTime.Expired(Runner))
            {
                Cook();
                nextCookTime = TickTimer.CreateFromSeconds(Runner, 10f);
            }
        }
        else
        {
            //타이머 초기화
            isCooking = false;
        }
    }

    private void Cook()
    {
        Debug.Log("요리 완성!");
        // 실제 요리 처리 로직
        //모닥불 UI가 닫혀있을 때
        if (campfire.CanOpen)
        {
            Item item = new Item
            {
                itemID = cookPotItem.itemID,
                count = cookPotItem.count - 1
            };
            cookPotItem = item;

            Item result = new Item
            {
                itemID = 7,//구운 고기
                count = resultItem.count + 1
            };
            resultItem = result;
        }
        else//모닥불 UI가 열려 있을 때   
        {
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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddFireTime(float time)
    {
        fireTime += time;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetisFire(bool tof)
    {
        
        if (!isFire && tof == true)
        {
            fireDecreaseTimer = TickTimer.CreateFromSeconds(Runner, 1f);
        }
        //isFire = tof;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetFireVFX(bool tof)
    {
        fireVFX.SetActive(tof);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Time(bool tof)
    {
        fireVFX.SetActive(tof);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetTimerText(int min, int sec)
    {
        campfire.canvasHolder.campfireUI.GetComponent<UICampfire>().SetTimerText(min, sec);
    }

    public float RemainingCookTime
    {
        get
        {
            if (!isCooking || !nextCookTime.IsRunning)
                return 0f;

            // TickTimer.RemainingTime은 double? 타입이라 null 체크 필요
            var remaining = nextCookTime.RemainingTime(Runner);
            return remaining.HasValue ? Mathf.Max(0f, (float)remaining.Value) : 0f;
        }
    }


    public float RemainingFireTime
    {
        get
        {
            if (!isFire) return 0f;
            return fireTime;
        }
    }
}