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

public class CwPlayer : MonoBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public Rigidbody rigid;
    [HideInInspector] public PlayerInputManager inputManager;

    [Header("Move")]
    [SerializeField] public float walkSpeed = 2f;
    [SerializeField] public float runSpeed = 4f;
    [SerializeField] public float crouchSpeed = 1f;
    [SerializeField] public float jumpForce = 5f;
    [SerializeField] public float airMoveSpeed = 3f;
    [SerializeField] public float rotationSpeed = 15f;

    private Dictionary<WeaponType, Weapon> weaponDict;
    public Weapon currentWeapon { get; private set; }
    public MoveSpeedState moveSpeedState;

    public PlayerData playerData;

    static public CwPlayer instance;

    private void Awake()
    {
        Singleton();

        InitializeComponents();
        InitializeWeapon();

        SetWeapon(WeaponType.Fist);
    }

    private void InitializeComponents()
    {
        rigid = GetComponent<Rigidbody>();
        inputManager = GetComponent<PlayerInputManager>();
    }

    private void Singleton()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
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
