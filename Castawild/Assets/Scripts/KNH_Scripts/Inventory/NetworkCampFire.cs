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
    [Networked] public bool isFire { get; set; } = false;
    [SerializeField] GameObject fireVFX;

    [Header("시간")]
    [Networked] public float fireTime { get; set; }
    [Networked] public float nextCookTime { get; set; }

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
    public void Update()
    {
        if (player == null) return;
        if (player.HasInputAuthority)
        {
            RPC_RequestSetTimerText();
        }
        if(player.HasStateAuthority)
        {
            if (inventoryData == null) return;

            // 불이 켜져 있을 때 fireTime 감소 처리
            if (isFire)
            {
                RPC_SetFireVFX(true);
                // 1초마다 감소
                if (Runner.SimulationTime >= nextFireDecreaseTime)
                {
                    Debug.Log("fire Time : " + fireTime);
                    fireTime -= 1;
                    int min = (int)fireTime / 60;
                    int sec = (int)fireTime % 60;
                    RPC_SetTimerText(min, sec);
                    nextFireDecreaseTime = Runner.SimulationTime + 1.0;

                    if (fireTime <= 0)
                    {
                        fireTime = 0;
                        isFire = false;
                        isCooking = false; // 불 꺼졌으니 요리 중지
                        RPC_SetFireVFX(false);
                        return; // 불 꺼졌으면 아래 요리 로직은 실행 안 함
                    }
                }
            }
            else
            {
                if (fireTime > 0) isFire = true;
                isCooking = false;
                RPC_SetFireVFX(false);
                return;
            }

            // 아이템이 있을 때만 타이머 작동
            if (inventoryData.itemList[45].itemID != -1 || cookPotItem.itemID != -1)
            {
                if (!isCooking)
                {
                    // 처음 조건 만족 시 타이머 시작
                    nextCookTime = Runner.SimulationTime + 10f;
                    isCooking = true;
                }

                ////fillAmount 갱신
                //if (isCooking)
                //{
                //    double totalDuration = 10.0;
                //    double elapsed = totalDuration - (nextCookTime - Runner.SimulationTime);
                //    float progress = Mathf.Clamp01((float)(elapsed / totalDuration));
                //}

                // 10초가 지났으면 Cook 실행 후 다음 시간 예약
                if (Runner.SimulationTime >= nextCookTime)
                {
                    Cook();
                    nextCookTime = Runner.SimulationTime + 10f; // 다음 10초 예약
                }
            }
            else
            {
                //타이머 초기화
                isCooking = false;
            }
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
        isFire = tof;
        RPC_SetFireVFX(tof);
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
        Debug.Log("RPC_SetTimerText");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestSetTimerText()
    {
        int min = (int)fireTime / 60;
        int sec = (int)fireTime % 60;
        campfire.canvasHolder.campfireUI.GetComponent<UICampfire>().SetTimerText(min, sec);
        Debug.Log("클라이언트에서 요청");
    }

    public float RemainingCookTime
    {
        get
        {
            if (!isCooking) return 0f;
            return Mathf.Max(0f, (float)(nextCookTime - Runner.SimulationTime));
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