using System.Collections.Generic;
using UnityEngine;
public enum WeaponType
{
    Fist,
    Throw,
    Sword,
    Bow
}

public enum MoveSpeedState { Walk, Run }

public class CwPlayer : CwCharacter
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public Rigidbody rigid;
    [HideInInspector] public PlayerInputManager inputManager;
    [HideInInspector] public PlayerMovement playerMovement;

    #region State
    public PlayerIdleState idleState;
    public PlayerMoveState moveState;
    public PlayerCrouchState crouchState;
    public PlayerInAir inAirState;

    public PlayerStateMachine stateMachine;
    #endregion

    [Header("Move")]
    [SerializeField] public float walkSpeed = 2f;
    [SerializeField] public float runSpeed = 4f;
    [SerializeField] public float crouchSpeed = 1f;
    [SerializeField] public float jumpForce = 5f;
    [SerializeField] public float airMoveSpeed = 3f;
    [SerializeField] public float rotationSpeed = 15f;

    [Header("Ground")]
    public LayerMask groundLayer;
    public float rayDistance = 1.1f;
    public Transform groundCheck;
    [HideInInspector] public bool canJump = true;
    [HideInInspector] public bool isGround = true;
    [HideInInspector] public bool isFalling = false;

    private Dictionary<WeaponType, Weapon> weaponDict;
    public Weapon currentWeapon { get; private set; }
    public MoveSpeedState moveSpeedState;

    [HideInInspector] public PlayerData playerData;

    static public CwPlayer instance;

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
        rigid = GetComponent<Rigidbody>();
        inputManager = GetComponent<PlayerInputManager>();
        playerMovement = GetComponent<PlayerMovement>();
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
    }

    private void InitializeWeapon()
    {
        weaponDict = new Dictionary<WeaponType, Weapon>
        {
            { WeaponType.Fist, new Melee() },
            { WeaponType.Throw, new Throw() },
            { WeaponType.Bow, new Bow() },
            { WeaponType.Sword, new Sword() }
        };
    }

    private void Update()
    {
        inputManager.HandleAllInputs();
        stateMachine?.currentState.UpdateState();
        isGround = Physics.Raycast(groundCheck.position, Vector3.down, rayDistance, groundLayer);
    }

    private void FixedUpdate()
    {
        playerMovement.HandleAllMovement();
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
    public void ChangePlayerMoveState(MoveSpeedState _moveState)
    {
        if (_moveState == MoveSpeedState.Walk)
        {
            moveSpeedState = MoveSpeedState.Walk;
            anim.SetBool("isRunning", false);
        }
        else
        {
            moveSpeedState = MoveSpeedState.Run;
            anim.SetBool("isRunning", true);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(groundCheck.position, Vector3.down * rayDistance);
    }

    public override void TakeDamage(float _damage)
    {
        base.TakeDamage(_damage);
    }

    protected override void Die()
    {

    }

    protected override void StatusEffect()
    {

    }

    public void InitializeData(PlayerData data)
    {
        CharacterName = data.characterName;
        MaxHp = data.maxHp;
        CurrentHp = MaxHp;
        Armor = data.armor;
        Attack = data.attack;
        playerData = data;

        anim = GetComponentInChildren<Animator>();
        stateMachine = new PlayerStateMachine();
        stateMachine.currentState = idleState;
    }

    /// <summary>
    /// 음식같은걸로 속도 바꿀 때 호출
    /// </summary>
    public void ChangeMoveSpeedValues(float value, bool isIncreasing)
    {
        if (isIncreasing)
        {
            walkSpeed += value;
            runSpeed += value;
            crouchSpeed += value;
            airMoveSpeed += value;
        }
        else
        {
            walkSpeed -= value;
            runSpeed -= value;
            crouchSpeed -= value;
            airMoveSpeed -= value;
        }
    }

}
