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
    public bool isCooking => cookTimer > 0;
    [SerializeField] GameObject fireVFX;

    [Header("시간")]
    [Networked] public float fireTime { get; set; }
    [Networked] public float cookTimer { get; set; }
    [Networked] private TickTimer fireDecreaseTimer { get; set; }
    [Networked] public TickTimer cookDecreaseTimer { get; set; }



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
                RPC_SetTimerText(min, sec);// 타이머 텍스트 업데이트
                if (cookDecreaseTimer.IsRunning)
                {
                    RPC_SetCookingProgressBar();//요리 진행바 업데이트
                    cookTimer--;
                    Debug.Log("cookTime : " + cookTimer);
                }

                //Debug.Log("fireTime : " + fireTime);
                if (fireTime <= 0)
                {
                    RPC_SetFireVFX(false);
                    return; // 불 꺼졌으면 아래 요리 로직은 실행 안 함
                }
            }
        }
        else
        {
            if (Object.HasStateAuthority)
                RPC_SetFireVFX(false);
            return;
        }

        //요리!!
        if (inventoryData == null) return;
        // 아이템이 있을 때만 타이머 작동
        if (inventoryData.itemList[45].itemID != -1 || cookPotItem.itemID != -1)
        {
            if (cookDecreaseTimer.ExpiredOrNotRunning(Runner))
            {
                cookTimer += 10;
            }
        }
        if (cookTimer > 0 && Object.HasStateAuthority)
        {
            // 처음 시작할 때만 타이머 생성
            if (!cookDecreaseTimer.IsRunning)
            {
                cookDecreaseTimer = TickTimer.CreateFromSeconds(Runner, 10f);
            }
            // 10초가 지났으면 Cook 실행 후 다음 시간 예약
            if (cookDecreaseTimer.ExpiredOrNotRunning(Runner))
            {
                Debug.Log("여기");
                cookDecreaseTimer = TickTimer.CreateFromSeconds(Runner, 10f);
                cookTimer = 10;
                RPC_Cooking();
                RPC_ResetCookingProgressBar();
                Debug.Log("요리 완성!");
            }
        }

    }

    private void Cook()
    {
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetCookingProgressBar()
    {
        campfire.canvasHolder.campfireUI.GetComponent<UICampfire>().
            CookingProgressBar(cookTimer);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Cooking()
    {
        Cook();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ResetCookingProgressBar()
    {
        campfire.canvasHolder.campfireUI.GetComponent<UICampfire>().ResetCookingProgressBar();
    }
}