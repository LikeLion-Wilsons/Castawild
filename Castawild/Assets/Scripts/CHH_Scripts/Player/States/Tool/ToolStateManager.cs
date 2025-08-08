using Fusion;
using System.Collections.Generic;
using UnityEngine;

// 현재 들고있는 무기
public enum ToolType { None, Fist, Throw, Spear, Sword, Bow, Axe, Pickaxe, Knife, Smash }

public enum ToolState { None, Idle, Aim, UseTool, Carry, Eat, Drink }
// 재생해야할 애니메이션 상태
public enum ToolAnimationState { None, Idle, Aim, FullAim, UseTool, Carry, Eat, Drink }

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
    public ToolBaseState currentState;
    #endregion

    [Header("Player")]
    public Transform armature;
    [SerializeField] private GameObject Client_armMesh;

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
    public HashSet<Transform> Host_alreadyHit = new HashSet<Transform>();
    private bool Host_canHit;

    #region Network
    [Header("Network")]
    [Networked, OnChangedRender(nameof(OnCurrentToolStateChanged))]
    public ToolState CurrentToolState { get; set; }
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

    public override void Spawned()
    {
        Host_ChangeState(ToolState.Idle);
        CurrentToolType = ToolType.Fist;
    }

    public override void FixedUpdateNetwork()
    {
        if (Host_canHit && HasStateAuthority && CurrentToolType == ToolType.Fist)
            Host_FistAttack();
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

    /// <summary>
    /// 상태 변환
    /// </summary>
    public void Host_ChangeState(ToolState newState)
    {
        if (!HasStateAuthority)
            return;

        currentState?.ExitState();
        CurrentToolState = newState;
        currentState = toolStateDict[CurrentToolState];
    }

    private void OnCurrentToolStateChanged()
    {
        if (toolStateDict.TryGetValue(CurrentToolState, out var newState))
        {
            currentState?.ExitState();
            currentState = newState;
            currentState.EnterState();
        }
    }

    /// <summary>
    /// 주먹 공격
    /// </summary>
    public void Host_FistAttack()
    {
        Collider[] hitObjects = Physics.OverlapBox(fistPos.position, hitBox, fistPos.rotation);

        for (int i = 0; i < hitObjects.Length; i++)
        {
            Transform hitObject = hitObjects[i].transform.root;

            if (hitObject.transform.root == this.transform.root)
                continue;

            if (Host_alreadyHit.Contains(hitObject.transform.root))
                continue;

            if (player.CanPVP && hitObject.TryGetComponent(out Player otherPlayer))
            {
                otherPlayer.Host_TakeDamage(true, player.All_GetToolAtt());
                Host_alreadyHit.Add(otherPlayer.transform.root);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(fistPos.position, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, hitBox * 2f);
    }

    /// <summary>
    /// 때릴 수 있게 설정
    /// </summary>
    public void Host_StartHit()
    {
        if (!HasStateAuthority)
            return;
        Host_canHit = true;
    }

    /// <summary>
    /// 때린거 초기화
    /// </summary>
    public void Host_FinishHit()
    {
        if (!HasStateAuthority)
            return;
        Host_canHit = false;
        Host_alreadyHit.Clear();
    }

    /// <summary>
    /// 애니메이션 업데이트
    /// </summary>
    public void All_UpdateMoveAnimation()
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
            case ToolAnimationState.UseTool:
                if (input.IsDown(PlayerNetworkInputData.aimInput) && All_HoldAimTool())
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
                anim.SetTrigger("Eating");
                CurrentToolAnimationState = ToolAnimationState.None;
                break;
            case ToolAnimationState.Drink:
                anim.SetTrigger("Drinking");
                CurrentToolAnimationState = ToolAnimationState.None;
                break;
        }

        anim.SetBool("ComboAttack", ComboAttack);
    }

    /// <summary>
    /// 팔 Mesh 활성화/비활성화
    /// </summary>
    public void Client_ArmVisibleChanged(bool isVisible)
    {
        if (cameraManager.currentView != ViewType.FirstPerson || !HasInputAuthority)
            return;

        Client_armMesh.SetActive(isVisible);
    }

    /// <summary>
    /// 공격 무기 들고있는지 확인
    /// </summary>
    public bool All_HoldAttackTool()
    {
        if (CurrentToolType == ToolType.Throw || CurrentToolType == ToolType.Fist || CurrentToolType == ToolType.Spear || CurrentToolType == ToolType.Sword)
            return true;
        else
            return false;
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

    /// <summary>
    /// 돌맹이/화살 생성
    /// </summary>
    public void Host_SpawnThrowObject(bool isArrow, Vector3 rayTargetPos)
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

    /// <summary>
    /// 스태미나 회복 가능한지 확인
    /// </summary>
    public bool All_CanRecoverStamina() => CurrentToolState != ToolState.UseTool;

    /// <summary>
    /// 현재 조준중인지 확인
    /// </summary>
    public bool All_IsAiming() => CurrentToolState == ToolState.Aim;

    /// <summary>
    /// Ray로 조준 위치 설정
    /// </summary>
    public void Client_SetTargetPos(int isArrow)
    {
        if (isArrow == 1)
            All_SetArrowPull(false);

        if (!HasInputAuthority)
            return;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        Vector3 rayTargetPos = ray.GetPoint(30f);
        RPC_RequestThrow(isArrow, rayTargetPos);
    }

    /// <summary>
    /// Aim 카메라,UI 설정
    /// </summary>
    public void Client_SetAimCameraAndUI(bool aimStart)
    {
        if (!HasInputAuthority)
            return;

        cameraManager.MoveAimCamera(aimStart);
        player.playerInteractUI.SetAimCrosshair(aimStart);
    }

    /// <summary>
    /// 활 조준 설정
    /// </summary>
    public void All_SetArrowPull(bool isAiming)
    {
        bowAnim.SetBool("Pull", isAiming);
        player.All_SetArrowActive(isAiming);
    }

    /// <summary>
    /// 활 쏘는 애니메이션 트리거
    /// </summary>
    public void All_BowShootAnimation() => bowAnim.SetTrigger("Shoot");

    /// <summary>
    /// 현재 아이템 변경 RPC
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_NotifyChangeSelectedItem(int itemIdx = -1)
    {
        // 설치가능한 아이템 
        if (itemIdx >= 300 && itemIdx < 400)
        {
            if (HasStateAuthority)
                Host_ChangeState(ToolState.Carry);
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
    /// 던지기
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestThrow(int isArrow, Vector3 rayTargetPos)
    {
        Host_SpawnThrowObject(isArrow == 0 ? false : true, rayTargetPos);

        if (isArrow == 0)
            player.All_SetCurrentToolActive(false);
        else
            player.arrow.SetActive(false);
    }
}
