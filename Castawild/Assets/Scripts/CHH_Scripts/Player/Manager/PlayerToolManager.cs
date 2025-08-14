using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

// 현재 들고있는 무기
public enum ToolType { None, Fist, Throw, Spear, Sword, Bow, Axe, Pickaxe }

[DisallowMultipleComponent]
public class PlayerToolManager : NetworkBehaviour
{
    private Player player;
    private PlayerCameraManager cameraManager;
    private PlayerFlagManager flagManager;

    #region Tool
    [Header("Tool")]
    [SerializeField] private Transform tools;
    private Dictionary<int, GameObject> toolDict = new Dictionary<int, GameObject>();
    [SerializeField] private GameObject emptyCup;
    private GameObject currentToolObject;
    [Networked] public ToolType CurrentToolType { get; set; } // 지금 들고있는 도구 타입, 상태 관련 -> 애니메이션 재생
    [Networked] public ToolInfoData currentToolInfoData { get; set; } // 들고있는 도구 정보 -> 공격력, 내구도 등

    [Header("Bow")]
    [SerializeField] private Transform bowOriginalParent;
    [SerializeField] private Transform bowUseParent;
    [SerializeField] private Transform bowUseLocalParent;
    public GameObject arrow;

    [Networked, HideInInspector] public bool HasArrow { get; set; }
    [Networked, HideInInspector] public bool HasPebble { get; set; }
    #endregion

    public event Action<int> Host_ChangeSelectedItem;

    private int prevItemIdx = -1;
    private int currentItemIdx = -1;

    private void Awake()
    {
        player = GetComponent<Player>();
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
        flagManager = GetComponent<PlayerFlagManager>();
    }

    public override void Spawned()
    {
        InitTools();
        player.ClearCup -= ChangeToEmptyCup;
        player.ClearCup += ChangeToEmptyCup;
    }

    private void ChangeToEmptyCup()
    {
        currentToolObject.SetActive(false);
        emptyCup.SetActive(true);
        currentToolObject = emptyCup;

        FoodInfo foodInfo = emptyCup.GetComponent<FoodInfo>();
        RPC_RequestSetCurrentFood(foodInfo.GetData());
        ToolInfo toolInfo = emptyCup.GetComponent<ToolInfo>();
        RPC_RequestSetCurrentTool(toolInfo.GetData());
    }

    public override void FixedUpdateNetwork()
    {
        if (flagManager.IsDead)
            return;

        if (HasStateAuthority)
        {
            if (currentItemIdx != prevItemIdx)
            {
                prevItemIdx = currentItemIdx;
                Host_ChangeSelectedItem?.Invoke(currentItemIdx);
            }
        }
    }

    private void InitTools()
    {
        foreach (Transform tool in tools)
        {
            ToolInfo itemInfo = tool.GetComponent<ToolInfo>();
            if (itemInfo != null)
            {
                if (!toolDict.ContainsKey(itemInfo.ItemID))
                {
                    toolDict.Add(itemInfo.ItemID, tool.gameObject);
                    tool.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 도구 장착
    /// </summary>
    public void Client_ApplySelectedItem(int itemIdx)
    {
        RPC_NotifySetCurrentItemType(itemIdx);

        // 도구일 경우 장착
        if (player.currentItemType == ItemType.Tool || player.currentItemType == ItemType.Drink || player.currentItemType == ItemType.Food)
        {
            if (toolDict.TryGetValue(itemIdx, out GameObject currentToolGameObject))
            {
                if (HasInputAuthority)
                    RPC_NotifyEquipmentTool(itemIdx);

                ToolInfo toolInfo = currentToolGameObject.GetComponent<ToolInfo>();
                RPC_RequestSetCurrentTool(toolInfo.GetData());

                if (player.currentItemType == ItemType.Drink || player.currentItemType == ItemType.Food)
                {
                    FoodInfo foodInfo = currentToolGameObject.GetComponent<FoodInfo>();
                    RPC_RequestSetCurrentFood(foodInfo.GetData());
                }
            }
            else
                Debug.LogWarning($"{itemIdx} 인덱스 없음");
        }
        else
        {
            RPC_NotifyEquipmentTool();
            RPC_RequestSetCurrentTool(ToolInfoData.Empty);
            RPC_RequestSetCurrentFood(FoodInfoData.Empty);
        }

        RPC_RequestChangeItemIdx(itemIdx);
    }

    /// <summary>
    /// 도구 해제
    /// </summary>
    public void All_RemoveSelectedItem()
    {
        RPC_NotifySetCurrentItemType(-1);
        RPC_NotifyEquipmentTool();
        RPC_RequestSetCurrentTool(ToolInfoData.Empty);
        RPC_RequestSetCurrentFood(FoodInfoData.Empty);

        RPC_RequestChangeItemIdx(-1);
    }

    /// <summary>
    /// 활 있는지
    /// </summary>
    public void Host_SetHasArrow(NetworkBool hasArrow) => HasArrow = hasArrow;

    /// <summary>
    /// 던지는 돌맹이 있는지
    /// </summary>
    public void Host_SetHasPebble(NetworkBool hasPebble) => HasPebble = hasPebble;

    /// <summary>
    /// 활 있는지
    /// </summary>
    public void RPC_RequestSetHasArrow(NetworkBool hasArrow) => HasArrow = hasArrow;

    /// <summary>
    /// 던지는 돌맹이 있는지
    /// </summary>
    public void RPC_RequestSetHasPebble(NetworkBool hasPebble) => HasPebble = hasPebble;

    /// <summary>
    /// 화살 위치 설정
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotifySetBowPos(bool isBowUse)
    {
        if (currentToolObject == null)
            return;

        if (HasInputAuthority)
        {
            if (isBowUse && cameraManager.currentView == ViewType.FirstPerson)
                currentToolObject.transform.SetParent(bowUseLocalParent);
            else if (isBowUse && cameraManager.currentView == ViewType.ThirdPerson)
                currentToolObject.transform.SetParent(bowUseParent);
            else
                currentToolObject.transform.SetParent(bowOriginalParent);
        }
        else
        {
            if (isBowUse)
                currentToolObject.transform.SetParent(bowUseParent);
            if (!isBowUse)
                currentToolObject.transform.SetParent(bowOriginalParent);
        }

        currentToolObject.transform.localPosition = Vector3.zero;
        currentToolObject.transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// 화살 활성화
    /// </summary>
    public void All_SetArrowActive(bool isBowUse)
    {
        if (HasArrow && isBowUse)
            arrow.SetActive(isBowUse);
        else
            arrow.SetActive(false);
    }

    /// <summary>
    /// 화살 활성화
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotifyArrowActive(bool isActive) => arrow.SetActive(false);

    public void Host_InitCurrentTool()
    {
        currentToolInfoData = ToolInfoData.Empty;

        RPC_NotifyInitCurrentToolObject();
    }

    /// <summary>
    /// 현재 도구 활성화
    /// </summary>
    public void All_SetPebbleActive(bool active)
    {
        if (HasPebble && active)
            currentToolObject?.SetActive(true);
        else if (!active)
            currentToolObject?.SetActive(false);
    }

    /// <summary>
    /// 현재 들고있는 도구 + 플레이어 공격력
    /// </summary>
    public int All_GetToolAtt(string toolName = "")
    {
        if (currentToolInfoData.IsEmpty())
            return player.playerData.attack;

        if (currentToolInfoData.toolName.Contains(toolName))
            return player.playerData.attack + currentToolInfoData.att;
        else if ((currentToolInfoData.itemID > 400 && currentToolInfoData.itemID < 407) || currentToolInfoData.itemID == 202)
            return player.playerData.attack + 2;
        else
            return player.playerData.attack;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_NotifySetCurrentItemType(int _currentItemIdx)
    {
        if (_currentItemIdx == 202)
            player.currentItemType = ItemType.Tool;
        // 407, 203, 204 : Drink
        else if (_currentItemIdx == 407 || _currentItemIdx == 203 || _currentItemIdx == 204)
            player.currentItemType = ItemType.Drink;
        // 6, 7 : Food
        else if (_currentItemIdx == 6 || _currentItemIdx == 7)
            player.currentItemType = ItemType.Food;
        // 300 ~ 400 : Placeable
        else if (_currentItemIdx >= 300 && _currentItemIdx < 400)
            player.currentItemType = ItemType.Placeable;
        // 400 ~ : Tool
        else if (_currentItemIdx >= 400)
            player.currentItemType = ItemType.Tool;
        else
            player.currentItemType = ItemType.Default;

        if (_currentItemIdx == 402)
            player.isNearFire++;
        else
            player.isNearFire--;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_NotifyEquipmentTool(int itemIdx = -1)
    {
        All_AllToolInActive();

        if (itemIdx == -1)
            return;

        if (toolDict.TryGetValue(itemIdx, out GameObject currentToolGameObject))
        {
            currentToolGameObject.SetActive(true);
            currentToolObject = currentToolGameObject;
        }
        else
        {
            currentToolObject = null;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestChangeItemIdx(int idx)
    {
        currentItemIdx = idx;
        Debug.Log("currentItemIdx : " + currentItemIdx);
        Debug.Log("prevItemIdx : " + prevItemIdx);
    }

    private void All_AllToolInActive()
    {
        foreach (var tool in toolDict)
        {
            if (tool.Value != null)
                tool.Value.SetActive(false);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestSetCurrentTool(ToolInfoData toolInfoData)
        => currentToolInfoData = toolInfoData.IsEmpty() ? ToolInfoData.Empty : toolInfoData;

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestSetCurrentFood(FoodInfoData foodInfoData)
        => player.currentFoodInfoData = foodInfoData.IsEmpty() ? FoodInfoData.Empty : foodInfoData;

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotifyInitCurrentToolObject()
    {
        currentToolObject.SetActive(false);
        currentToolObject = null;

        All_AllToolInActive();
    }

    /// <summary>
    /// 곡괭이/도끼 들고있는지 확인
    /// </summary>
    public bool All_HoldCraftingTool()
    {
        if (CurrentToolType == ToolType.Axe || CurrentToolType == ToolType.Pickaxe)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 조준가능한 도구인지 확인
    /// </summary>
    public bool All_HoldAimTool() => CurrentToolType == ToolType.Bow || CurrentToolType == ToolType.Throw;


    public bool All_IsDecreaseDurationTool()
    {
        if (CurrentToolType == ToolType.Fist || CurrentToolType == ToolType.Bow || CurrentToolType == ToolType.Throw)
            return false;
        else
            return true;
    }

}