using Fusion;
using UnityEngine;

public class Campfire : InteractableObject
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

    [Networked] public bool CanOpen { get; set; } = true;
    public UI_Manager canvasHolder;
    public Player player;
    public InventoryDataManager inventoryData;
    [SerializeField] CampFireObject warmObject;

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

        InventoryDataManager.cookStart -= RPC_AddCookTime;
        InventoryDataManager.cookStart += RPC_AddCookTime;
    }

    private void Awake()
    {
        interactableType = InteractableType.Box;
        isPlaceable = true;
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
                    warmObject.FinishFire();
                    return;
                }

                if (cookTimer > 1 && canCook)
                { // 1초마다 cookTimer 감소
                    cookTimer--;
                    // 진행 바 업데이트
                    RPC_SetCookingProgressBar(cookTimer);
                    Debug.Log("cookTime : " + cookTimer);
                }
                else if (cookTimer == 1 && canCook)
                {
                    // 요리 완료
                    cookTimer = -1;
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
        Debug.Log(CanOpen);

        int GetCookedID(int rawID)
        {
            return rawID switch
            {
                6 => 7,      // 생고기 → 구운 고기
                203 => 204,  // 203 → 204
                _ => rawID   // 그 외는 그대로
            };
        }

        if (CanOpen)//닫혀있을 때
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
            if (player == null) return;
            if (player.HasStateAuthority)
            {
                RPC_SetCookPotItem(inventoryData.itemList[45]);
                RPC_SetResultItem(inventoryData.itemList[46]);
            }
            Item item = new Item
            {
                itemID = cookPotItem.itemID,
                count = cookPotItem.count - 1
            };
            RPC_SetCookPotItem(item);

            Item result = new Item
            {
                itemID = GetCookedID(cookPotItem.itemID),
                count = resultItem.count + 1
            };
            RPC_SetResultItem(result);

            inventoryData.RPC_SetItemFromCampfire(this);

            if (cookPotItem.count > 0) RPC_AddCookTime(10);
            else canCook = false;
        }
    }

    public override bool CanInteract() => CanOpen;

    public override void Interact(PlayerRef playerRef)
    {
        NetworkObject playerObj = Runner.GetPlayerObject(playerRef);

        RPC_Interact(playerRef);

        if (CanOpen)
        {
            canvasHolder.uiParts["Inventory"].Open();
            canvasHolder.uiParts["Campfire"].Open();
            if (player.HasStateAuthority)
                CanOpen = false;
            else if (player.HasInputAuthority)
                inventoryData.RPC_SetCanOpen(this, false);
        }

    }

    // 클라 -> 서버
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Interact(PlayerRef playerRef)
    {
        NetworkObject playerObj = Runner.GetPlayerObject(playerRef);
        player = playerObj.GetComponent<Player>();
        inventoryData = player.GetComponent<InventoryDataManager>();
        inventoryData.RPC_SetItemFromCampfire(this);
        canvasHolder = inventoryData.canvasHolder;
        canvasHolder.currentCampFire = gameObject;

    }

    public void FinishInteract()
    {
        int index = 29;

        if (player.HasStateAuthority)        //호스트에서
        {
            CanOpen = true;

            Debug.Log("호스트 inventory -> campfire");

            cookPotItem = inventoryData.itemList[45];
            resultItem = inventoryData.itemList[46];
            inventoryData.RPC_UpdateInventoryUI();
        }
        else if (player.HasInputAuthority) //클라이언트에서
        {
            inventoryData.RPC_SetCanOpen(this, true);
            Debug.Log("클라이언트 inventory -> campfire");
            inventoryData.RPC_RequestStoreToCampfire(this);
        }

        index = 0;
        //inventory 초기화
        Item item = new Item
        {
            itemID = -1,
            count = 0,
            durability = 1
        };
        for (int i = 45; i < 47; i++)
        {
            player.GetComponent<InventoryDataManager>().RPC_SetItem(i, item);
            index++;
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
        warmObject.gameObject.SetActive(true);
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
        if (canvasHolder == null || canvasHolder.campfireUI == null)
            return;
        canvasHolder.campfireUI.GetComponent<UICampfire>().SetTimerText(min, sec);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetCookingProgressBar(float timer)
    {
        if (canvasHolder == null || canvasHolder.campfireUI == null)
            return;
        canvasHolder.campfireUI.GetComponent<UICampfire>().
            CookingProgressBar(timer);
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetFireIcon(bool tof)
    {
        if (canvasHolder == null || canvasHolder.campfireUI == null)
            return;
        canvasHolder.campfireUI.GetComponent<UICampfire>().
            SetFireIcon(tof);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Cooking()
    {
        if (!Object.HasStateAuthority) return; // 클라는 실행하지 않음
        Cook();
    }
}
