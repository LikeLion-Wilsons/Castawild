using Fusion;
using UnityEngine;

public enum ToolAnimationState { Idle, Aim, FullAim, FullUse }
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
    #endregion

    public Transform armature;
    public GameObject visibleMesh;

    #region Network
    [Networked] public bool ComboAttack { get; set; }
    [Networked] public bool CanReceiveInput { get; set; }
    [Networked] public ToolAnimationState CurrentToolUseState { get; set; }
    [Networked] public ToolType CurrentToolType { get; set; }
    #endregion

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
        }

        anim.SetBool("ComboAttack", ComboAttack);
    }

    // 테스트용
    public void ChangeCurrentTool()
    {
        if (input.WasPressed(prevInputButtons, PlayerNetworkInputData.toolChangedInput))
        {
            int first = 1;
            int last = System.Enum.GetValues(typeof(ToolType)).Length - 1;

            int next = (int)CurrentToolType + 1;

            if (next > last)
                next = first;

            CurrentToolType = (ToolType)next;
        }
    }

    /// <summary>
    /// 공격 무기 들고있는지 확인
    /// </summary>
    public bool HoldTool()
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
}
