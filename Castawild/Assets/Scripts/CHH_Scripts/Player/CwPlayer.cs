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
    [HideInInspector] public MovementStateManager movementManager;
    [HideInInspector] public AimStateManger aimStateManager;

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


}
