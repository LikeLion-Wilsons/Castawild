using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Fist,
    Throw,
    Sword,
    Bow
}

public enum MoveState { Walk, Run }

public class Player : MonoBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public Rigidbody rigid;
    [HideInInspector] public PlayerInputController inputController;

    #region State
    public PlayerIdleState idleState;
    public PlayerMoveState moveState;
    public PlayerCrouchState crouchState;
    public PlayerInAir inAirState;

    public PlayerStateMachine stateMachine;
    #endregion

    [Header("Ground")]
    public LayerMask groundLayer;
    public float rayDistance = 1.1f;
    public Transform groundCheck;
    [HideInInspector] public bool isGrounded = true;
    [HideInInspector] public bool isJumping = false;
    [HideInInspector] public bool isFalling = false;

    private Dictionary<WeaponType, IWeapon> weaponDict;
    public IWeapon currentWeapon { get; private set; }

    static public Player instance;

    private void Awake()
    {
        Singleton();

        InitializeComponents();
        InitializeStates();
        InitializeWeapon();

        SetWeapon(WeaponType.Fist);
    }

    private void InitializeComponents()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        inputController = GetComponent<PlayerInputController>();
    }

    private void Singleton()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void InitializeStates()
    {
        idleState = new PlayerIdleState(this, stateMachine, "Idle");
        moveState = new PlayerMoveState(this, stateMachine, "Move");
        crouchState = new PlayerCrouchState(this, stateMachine, "Crouch");
        inAirState = new PlayerInAir(this, stateMachine, "InAir");

        stateMachine = new PlayerStateMachine();
        stateMachine.currentState = idleState;
    }

    private void InitializeWeapon()
    {
        weaponDict = new Dictionary<WeaponType, IWeapon>
        {
            { WeaponType.Fist, new Melee() },
            { WeaponType.Throw, new Throw() },
            { WeaponType.Bow, new Bow() },
            { WeaponType.Sword, new Sword() }
        };
    }

    private void Update()
    {
        stateMachine.currentState.UpdateState();
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, rayDistance, groundLayer);
    }

    /// <summary>
    /// 무기 바꿀 때 호출
    /// </summary>
    public void SetWeapon(WeaponType weaponType)
    {
        if (weaponDict.TryGetValue(weaponType, out var weapon))
            currentWeapon = weapon;
        else
            Debug.LogError($"Weapon type {weaponType} 없음");
    }

    /// <summary>
    /// 플레이어 상태 바꿀 때 호출
    /// </summary>
    public void ChangeStateMachine(PlayerState playerstate) => stateMachine.ChangeState(playerstate);

    /// <summary>
    /// 플레이어 이속 바꿀 때 호출
    /// </summary>
    public void ChangePlayerMoveState(MoveState _moveState)
    {
        if (_moveState == MoveState.Walk)
            anim.SetBool("isRunning", false);
        else
            anim.SetBool("isRunning", true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(groundCheck.position, Vector3.down * rayDistance);
    }

    private void LandAnimationFinishTrigger()
    {
        if (inputController.moveInput.magnitude > 0f)
            stateMachine.ChangeState(moveState);
        else
            stateMachine.ChangeState(idleState);
    }
}
