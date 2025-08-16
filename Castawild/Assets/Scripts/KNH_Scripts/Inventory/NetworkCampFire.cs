using Fusion;
using System;
using System.Collections.Generic;
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
    [Networked] public bool canCook { get; set; }
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
        InventoryDataManager.cookStart -= RPC_AddCookTime;
        InventoryDataManager.cookStart += RPC_AddCookTime;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return; // 🔥 불 처리
        if (fireTime > 0)
        {
            RPC_SetFireIcon(true);
            RPC_SetFireVFX(true);
            if (fireDecreaseTimer.ExpiredOrNotRunning(Runner))
            {
                //1초마다
                fireDecreaseTimer = TickTimer.CreateFromSeconds(Runner, 1f); fireTime--;
                int min = (int)fireTime / 60;
                int sec = (int)fireTime % 60;
                RPC_SetTimerText(min, sec);
                if (fireTime <= 0)
                {
                    RPC_SetFireVFX(false);
                    return;
                }

                if (cookTimer > 0 && canCook)
                { // 1초마다 cookTimer 감소
                    cookTimer--;
                    // 진행 바 업데이트
                    RPC_SetCookingProgressBar(cookTimer);
                    Debug.Log("cookTime : " + cookTimer);
                }
                else if (cookTimer == 0 && canCook)
                {
                    // 요리 완료
                    cookTimer = -1;
                    RPC_SetCookingProgressBar(0);
                    RPC_Cooking();
                }
                else if (!canCook) { RPC_SetCookingProgressBar(0); }
            }
        }
        else
        {
            RPC_SetFireIcon(false);
            RPC_SetFireVFX(false);
            return;
        }
    }

    private void Cook()
    {
        Debug.Log("요리 완성!");

        int GetCookedID(int rawID)
        {
            return rawID switch
            {
                6 => 7,      // 생고기 → 구운 고기
                203 => 204,  // 203 → 204
                _ => rawID   // 그 외는 그대로
            };
        }

        if (campfire.CanOpen)//닫혀있을 때
        {
            Item item = new Item
            {
                itemID = cookPotItem.itemID,
                count = cookPotItem.count - 1
            };
            cookPotItem = item;

            Item result = new Item
            {
                itemID = GetCookedID(cookPotItem.itemID),
                count = resultItem.count + 1
            };
            resultItem = result;
            if (cookPotItem.count > 0) RPC_AddCookTime(10);
            else canCook = false;
        }
        else//열려있을 때
        {
            Item item = new Item
            {
                itemID = inventoryData.itemList[45].itemID,
                count = inventoryData.itemList[45].count - 1
            };
            inventoryData.RPC_SetItem(45, item);

            Item result = new Item
            {
                itemID = GetCookedID(inventoryData.itemList[45].itemID),
                count = inventoryData.itemList[46].count + 1
            };
            inventoryData.RPC_SetItem(46, result);
            inventoryData.RPC_UpdateInventoryUI();
            if (inventoryData.itemList[45].count > 0) RPC_AddCookTime(10);
            else canCook = false;
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
    public void RPC_AddCookTime(float time)
    {
        if (cookTimer > 0)
            return; // 이미 요리 중이면 추가하지 않음
        Debug.Log("AddCookTime: " + time);  
        cookTimer += time;
        canCook = true;
        //인벤토리 -> 모닥불
        //inventoryData.RPC_RequestStoreToCampfire(this);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetFireVFX(bool tof)
    {
        fireVFX.SetActive(tof);
    }



    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetTimerText(int min, int sec)
    {
        if (campfire.canvasHolder == null || campfire.canvasHolder.campfireUI == null)
            return;
        campfire.canvasHolder.campfireUI.GetComponent<UICampfire>().SetTimerText(min, sec);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetCookingProgressBar(float timer)
    {
        if (campfire.canvasHolder == null || campfire.canvasHolder.campfireUI == null)
            return;
        campfire.canvasHolder.campfireUI.GetComponent<UICampfire>().
            CookingProgressBar(timer);
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetFireIcon(bool tof)
    {
        if (campfire.canvasHolder == null || campfire.canvasHolder.campfireUI == null)
            return;
        campfire.canvasHolder.campfireUI.GetComponent<UICampfire>().
            SetFireIcon(tof);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Cooking()
    {
        Cook();
    }
}