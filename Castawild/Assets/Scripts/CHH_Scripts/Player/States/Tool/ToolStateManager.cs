using Fusion;
using UnityEngine;

// 현재 들고있는 무기
public enum ToolType { None, Fist, Throw, Spear, Sword, Bow, Axe, Pickaxe, Knife, Smash }

// 재생해야할 애니메이션 상태
public enum ToolAnimationState { Idle, Aim, FullAim, FullUse, Carry, Eat, Drink }

public class ToolStateManager : BaseStateManager
{
    #region Components
    [HideInInspector] public MovementStateManager movementManager;
    [HideInInspector] public AnimationTrigger animTrigger;
    #endregion

    #region States
    public ToolIdleState idleState;
    public UseToolState useToolState;
    public AimState aimState;
    public CarryState carryState;
    public EatState eatState;
    #endregion

    public Transform armature;
    [SerializeField] private GameObject armMesh;

    #region Network
    [Networked] public bool CanComboAttack { get; set; }
    [Networked] public bool ComboAttack { get; set; }
    [Networked] public bool CanReceiveInput { get; set; }
    [Networked] public ToolAnimationState CurrentToolUseState { get; set; }
    [Networked] public ToolType CurrentToolType { get; set; }
    #endregion

    public bool isTriggerSet = false;

    protected override void Awake()
    {
        base.Awake();

        InitComponents();
        InitStates();
    }

    private void InitComponents()
    {
        movementManager = GetComponent<MovementStateManager>();
        animTrigger = GetComponentInChildren<AnimationTrigger>();
    }

    private void InitStates()
    {
        idleState = new ToolIdleState(this, inputManager);
        useToolState = new UseToolState(this, inputManager);
        aimState = new AimState(this, inputManager);
        carryState = new CarryState(this, inputManager);
        eatState = new EatState(this, inputManager);
    }

    public override void Spawned()
    {
        ChangeState(idleState);
        CurrentToolType = ToolType.Fist;
    }

    public void UpdateMoveAnimation()
    {
        anim.SetBool("Aiming", false);
        anim.SetBool("FullAiming", false);
        anim.SetBool("FullUseTool", false);
        anim.SetBool("Carrying", false);

        switch (CurrentToolUseState)
        {
            case ToolAnimationState.Aim:
                anim.SetInteger("WeaponType", (int)CurrentToolType);
                anim.SetBool("Aiming", true);
                break;
            case ToolAnimationState.FullAim:
                anim.SetInteger("WeaponType", (int)CurrentToolType);
                anim.SetBool("FullAiming", true);
                break;
            case ToolAnimationState.FullUse:
                anim.SetInteger("WeaponType", (int)CurrentToolType);
                anim.SetBool("FullUseTool", true);
                break;
            case ToolAnimationState.Carry:
                anim.SetBool("Carrying", true);
                break;
            case ToolAnimationState.Eat:
                if (!isTriggerSet)
                    anim.SetTrigger("Eating");
                isTriggerSet = true;
                break;
            case ToolAnimationState.Drink:
                if (!isTriggerSet)
                    anim.SetTrigger("Drinking");
                isTriggerSet = true;
                break;
        }

        anim.SetBool("ComboAttack", ComboAttack);
    }

    // 400:짱돌 401:방망이 402:횃불 403:돌도끼 404:돌작살 405:돌곡괭이
    public void ChangeSelectedItem(int itemIdx = -1)
    {
        // 설치가능한 아이템 
        if (itemIdx >= 300 && itemIdx < 400)
        {
            ChangeState(carryState);
            return;
        }

        switch (itemIdx)
        {
            case 401: // 방망이
                CurrentToolType = ToolType.Sword;
                break;
            case 402: // 횃불
                CurrentToolType = ToolType.Sword;
                break;
            case 403: // 돌도끼
                CurrentToolType = ToolType.Axe;
                break;
            case 404: // 돌작살
                CurrentToolType = ToolType.Spear;
                break;
            case 405: // 돌곡괭이
                CurrentToolType = ToolType.Pickaxe;
                break;
            case 400: // 400:짱돌 
            default:
                CurrentToolType = ToolType.Fist;
                break;
        }
    }

    /// <summary>
    /// 공격 무기 들고있는지 확인
    /// </summary>
    public bool HoldAttackTool()
    {
        if (CurrentToolType == ToolType.Throw || CurrentToolType == ToolType.Fist || CurrentToolType == ToolType.Spear || CurrentToolType == ToolType.Sword)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 곡괭이/도끼 들고있는지 확인
    /// </summary>
    public bool HoldCraftingTool()
    {
        if (CurrentToolType == ToolType.Axe || CurrentToolType == ToolType.Pickaxe)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 조준가능한 도구인지 확인
    /// </summary>
    public bool HoldAimTool() => CurrentToolType == ToolType.Bow || CurrentToolType == ToolType.Throw;

    public void ArmVisibleChanged(bool isVisible)
    {
        if (HasStateAuthority && cameraManager.currentView == ViewType.FirstPerson)
        {
            RPC_ArmVisibleChanged(isVisible);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ArmVisibleChanged(bool isVisible) => armMesh.SetActive(isVisible);
}
