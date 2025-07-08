using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { None, Fist, Throw, Sword, Bow }
public enum MoveType { Idle, Walk, Run, Crouch, Jump }
public enum AttackType { None, Aim, Attack }

public class CwPlayer : MonoBehaviour
{
    // 나중에 HideInInspector로 바꾸기
    /*[HideInInspector] */
    public Animator anim;
    [HideInInspector] public Rigidbody rigid;
    [HideInInspector] public PlayerInputManager inputManager;
    [HideInInspector] public MovementStateManager movementManager;
    [HideInInspector] public AttackStateManager attackStateManager;

    private Dictionary<WeaponType, Weapon> weaponDict;
    public WeaponType currentWeaponType;
    public MoveType currentMoveType;
    public AttackType currentAttackType;

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
        anim = GetComponentInChildren<Animator>();
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

    /// <summary>
    /// 무기 바꿀 때 호출
    /// </summary>
    public void SetWeapon(WeaponType weaponType)
    {
        currentWeaponType = weaponType;
        anim.SetInteger("WeaponType", (int)currentWeaponType);
    }

    /// <summary>
    /// 공격시 호출
    /// </summary>
    public void Attack()
    {
        anim.SetInteger("WeaponType", (int)currentWeaponType);

        if (weaponDict.TryGetValue(currentWeaponType, out Weapon weapon))
            weapon.Attack();

        else
            Debug.LogWarning("Weapon not found: " + currentWeaponType);
    }

    // 테스트용
    private void OnValidate()
    {
        anim.SetInteger("WeaponType", (int)currentWeaponType);
    }
}
