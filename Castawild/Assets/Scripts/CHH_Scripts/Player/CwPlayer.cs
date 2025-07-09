using UnityEngine;

// 테스트용
public enum ToolType { None, Fist, Throw, Sword, Bow, Axe, Pickaxe, Knife }
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
    [HideInInspector] public ToolStateManager toolStateManager;

    public ToolType currentToolType;
    public MoveType currentMoveType;
    public AttackType currentAttackType;

    public bool isAimLocked = false;

    public PlayerData playerData;

    static public CwPlayer instance;

    private void Awake()
    {
        Singleton();

        InitializeComponents();

        SetWeapon(ToolType.Fist);
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

    /// <summary>
    /// 무기 바꿀 때 호출
    /// </summary>
    public void SetWeapon(ToolType weaponType)
    {
        currentToolType = weaponType;
        anim.SetInteger("WeaponType", (int)currentToolType);
    }

    /// <summary>
    /// 공격시 호출
    /// </summary>
    public void Attack()
    {
        anim.SetInteger("WeaponType", (int)currentToolType);

        if (currentToolType == ToolType.Throw)
        {

        }
        else if (currentToolType == ToolType.Bow)
        {

        }
    }

    // 테스트용
    private void OnValidate()
    {
        anim.SetInteger("WeaponType", (int)currentToolType);
    }
}
