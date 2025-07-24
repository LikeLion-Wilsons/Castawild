using UnityEngine;

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

    public void UpdateMoveAnimation()
    {
        anim.SetBool("Aiming", false);
        anim.SetBool("FullAiming", false);
        anim.SetBool("UseTool", false);
        anim.SetBool("FullUseTool", false);

        switch (networkManager.CurrentToolUseState)
        {
            case ToolAnimationState.Aim:
                anim.SetBool("Aiming", true);
                break;
            case ToolAnimationState.FullAim:
                anim.SetBool("FullAiming", true);
                break;
            case ToolAnimationState.Use:
                anim.SetInteger("WeaponType", (int)networkManager.CurrentToolType);
                anim.SetBool("UseTool", true);
                break;
            case ToolAnimationState.FullUse:
                anim.SetInteger("WeaponType", (int)networkManager.CurrentToolType);
                anim.SetBool("FullUseTool", true);
                break;
        }
        anim.SetBool("ComboAttack", networkManager.ComboAttack);

        if (input.IsUp(PlayerNetworkInputData.aimInput))
        {
            anim.SetBool("Aiming", false);
            anim.SetBool("FullAiming", false);
            cameraManager.MoveCamera(false);
        }
    }

    // 테스트용
    public void ChangeCurrentTool()
    {
        if (input.WasPressed(prevInputButtons, PlayerNetworkInputData.toolChangedInput))
        {
            int first = 1;
            int last = System.Enum.GetValues(typeof(ToolType)).Length - 1;

            int next = (int)networkManager.CurrentToolType + 1;

            if (next > last)
                next = first;

            networkManager.CurrentToolType = (ToolType)next;
        }
    }

    public override void UpdateAnimationFlags()
    {
        base.UpdateAnimationFlags();
        networkManager.ComboAttack = comboAttack;
        networkManager.CanReceiveInput = animTrigger.canReceiveInput;
    }
}
