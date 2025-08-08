using Fusion;
using System.Collections.Generic;
using UnityEngine;

// 현재 들고있는 무기
public enum ToolType { None, Fist, Throw, Spear, Sword, Bow, Axe, Pickaxe, Knife, Smash }

public enum ToolState { None, Idle, Aim, UseTool, Carry, Eat, Drink }
// 재생해야할 애니메이션 상태
public enum ToolAnimationState { None, Idle, Aim, FullAim, FullUse, Carry, Eat, Drink }

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
    public Dictionary<ToolState, ToolBaseState> toolStateDict;
    public ToolBaseState currentState_Host; // 호스트용 변수
    #endregion

    [Header("Player")]
    public Transform armature;
    [SerializeField] private GameObject armMesh;

    [Header("Bow")]
    [SerializeField] private Animator bowAnim;

    [Header("Throw")]
    [SerializeField] private float throwUpForce = 0.3f;
    [SerializeField] private float arrowUpForce = 0.3f;
    [SerializeField] private float throwForce = 20f;
    [SerializeField] private float arrowForce = 30f;
    [SerializeField] private ThrowObject throwableStonePrefab;
    [SerializeField] private ThrowObject arrowPrefab;
    [SerializeField] private Transform firstPersonArrowPos;
    [SerializeField] private Transform thirdPersonArrowPos;
    [SerializeField] private Transform throwPos;

    [Header("Hit")]
    [SerializeField] private Transform fistPos;
    [SerializeField] private Vector3 hitBox = new Vector3(1f, 1f, 1.0f);
    public HashSet<Transform> alreadyHit = new HashSet<Transform>();
    private bool canHit;

    #region Network
    [Networked] public ToolState CurrentToolState { get; set; }
    [Networked] public ToolAnimationState CurrentToolAnimationState { get; set; }
    [Networked] public ToolType CurrentToolType { get; set; }
    [Networked, HideInInspector] public bool CanComboAttack { get; set; }
    [Networked, HideInInspector] public bool ComboAttack { get; set; }
    [Networked, HideInInspector] public bool CanReceiveInput { get; set; }

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
        carryState = new CarryState(this, inputManager);
        eatState = new EatState(this, inputManager);

        toolStateDict = new Dictionary<ToolState, ToolBaseState>
        {
            { ToolState.Idle, idleState },
            { ToolState.Aim, aimState },
            { ToolState.UseTool, useToolState },
            { ToolState.Carry, carryState },
            { ToolState.Eat, eatState }
        };
    }

    public override void Spawned()
    {
        ChangeState(ToolState.Idle);
        CurrentToolType = ToolType.Fist;
    }

    public override void FixedUpdateNetwork()
    {
        if (canHit && HasStateAuthority && CurrentToolType == ToolType.Fist)
            FistAttack();
    }

    public void FistAttack()
    {
        Collider[] hitObjects = Physics.OverlapBox(fistPos.position, hitBox, fistPos.rotation);

        for (int i = 0; i < hitObjects.Length; i++)
        {
            Transform hitObject = hitObjects[i].transform.root;

            if (hitObject.transform.root == this.transform.root)
                continue;

            if (alreadyHit.Contains(hitObject.transform.root))
                continue;

            if (player.CanPVP && hitObject.TryGetComponent(out Player otherPlayer))
            {
                otherPlayer.TakeDamage(true, player.GetToolAtt());
                alreadyHit.Add(otherPlayer.transform.root);
            }
        }
    }

    public void ChangeState(ToolState newState)
    {
        if (!HasStateAuthority)
            return;

        currentState_Host?.ExitState();
        CurrentToolState = newState;
        currentState_Host = toolStateDict[CurrentToolState];
        currentState_Host.EnterState();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(fistPos.position, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, hitBox * 2f);
    }

    public void StartHit()
    {
        canHit = true;
    }

    public void FinishHit()
    {
        canHit = false;
        alreadyHit.Clear();
    }

    public void UpdateMoveAnimation()
    {
        anim.SetBool("Aiming", false);
        anim.SetBool("FullAiming", false);
        anim.SetBool("FullUseTool", false);
        anim.SetBool("Carrying", false);

        switch (CurrentToolAnimationState)
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
                if (input.IsDown(PlayerNetworkInputData.aimInput) && HoldAimTool())
                {
                    anim.SetInteger("WeaponType", (int)CurrentToolType);
                    anim.SetBool("Aiming", true);
                    anim.SetBool("FullAiming", true);
                }
                anim.SetInteger("WeaponType", (int)CurrentToolType);
                anim.SetBool("FullUseTool", true);
                break;
            case ToolAnimationState.Carry:
                anim.SetBool("Carrying", true);
                break;
            case ToolAnimationState.Eat:
                if (!IsTriggerSet)
                    anim.SetTrigger("Eating");
                IsTriggerSet = true;
                break;
            case ToolAnimationState.Drink:
                if (!IsTriggerSet)
                    anim.SetTrigger("Drinking");
                IsTriggerSet = true;
                break;
        }

        anim.SetBool("ComboAttack", ComboAttack);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_ChangeSelectedItem(int itemIdx = -1)
    {
        // 설치가능한 아이템 
        if (itemIdx >= 300 && itemIdx < 400)
        {
            if (HasStateAuthority)
                ChangeState(ToolState.Carry);
            return;
        }

        switch (itemIdx)
        {
            case 202: // 짱돌 
                CurrentToolType = ToolType.Throw;
                break;
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
            case 406: // 활
                {
                    CurrentToolType = ToolType.Bow;
                    player.arrow.SetActive(false);
                }
                break;
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ArmVisibleChanged(bool isVisible)
    {
        if (cameraManager.currentView != ViewType.FirstPerson)
            return;

        armMesh.SetActive(isVisible);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_MoveAimCamera(bool _isAiming) => cameraManager.MoveCamera(_isAiming);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_BowAim(bool isAiming)
    {
        bowAnim.SetBool("Pull", isAiming);
        player.SetBowPos(isAiming);
        player.ActiveArrow(isAiming);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_BowShootAnimation() => bowAnim.SetTrigger("Shoot");

    public void SetTargetPos(int isArrow)
    {
        if (!HasInputAuthority)
            return;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        Vector3 rayTargetPos = ray.GetPoint(30f);
        RPC_Throw(isArrow, rayTargetPos);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Throw(int isArrow, Vector3 rayTargetPos)
    {
        SpawnThrowObject(isArrow == 0 ? false : true, rayTargetPos);

        if (isArrow == 0)
            player.CurrentToolActive(false);
        else
            player.arrow.SetActive(false);
    }

    // 돌맹이 / 화살 생성
    public void SpawnThrowObject(bool isArrow, Vector3 rayTargetPos)
    {
        if (!HasStateAuthority)
            return;

        NetworkObject throwObject;
        if (isArrow && player.HasArrow)
        {
            if (input.currentView == ViewType.FirstPerson)
            {
                throwObject = Runner.Spawn(arrowPrefab.gameObject, firstPersonArrowPos.position, cameraManager.firstPersonCam.transform.rotation);
                throwObject?.GetComponent<ThrowObject>().AddForce(arrowForce, arrowUpForce, rayTargetPos);
            }
            else
            {
                throwObject = Runner.Spawn(arrowPrefab.gameObject, thirdPersonArrowPos.position, cameraManager.thirdPersonCam.transform.rotation);
                throwObject?.GetComponent<ThrowObject>().AddForce(arrowForce, arrowUpForce, rayTargetPos);
            }
        }
        else if (!isArrow)
        {
            if (input.currentView == ViewType.FirstPerson)
                throwObject = Runner.Spawn(throwableStonePrefab.gameObject, throwPos.position, cameraManager.firstPersonCam.transform.rotation);
            else
                throwObject = Runner.Spawn(throwableStonePrefab.gameObject, throwPos.position, cameraManager.thirdPersonCam.transform.rotation);
            throwObject?.GetComponent<ThrowObject>().AddForce(throwForce, throwUpForce, rayTargetPos);
        }

        // 돌맹이나 화살 개수 줄이기
    }

    public bool CanRecoverStamina() => currentState_Host != useToolState;

    public bool IsAiming() => CurrentToolState == ToolState.Aim;

    public void StartAim(bool aimStart)
    {
        RPC_MoveAimCamera(aimStart);
        player.RPC_ActiveAimUI(aimStart);

        if (CurrentToolType == ToolType.Bow)
            RPC_BowAim(aimStart);
    }
}
