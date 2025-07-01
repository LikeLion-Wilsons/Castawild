using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum WeaponType
{
    Fist,
    Throw,
    Sword,
    Bow
}

public class Player : MonoBehaviour
{
    #region State
    private PlayerIdleState playerIdleState;
    private PlayerWalkState playerMoveState;
    private PlayerRunState playerRunState;
    private PlayerCrouchState playerCrouchState;
    private PlayerJumpState playerJumpState;
    private PlayerStateMachine playerStateMachine;
    #endregion

    private Dictionary<WeaponType, IWeapon> weaponDict;
    public IWeapon currentWeapon { get; private set; }

    static public Player instance;

    private void Awake()
    {
        Singleton();

        InitializeState();
        InitializeWeapon();

        SetWeapon(WeaponType.Fist);
    }

    private void Update()
    {
        playerStateMachine.currentState.UpdateState();
    }

    private void Singleton()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void InitializeState()
    {
        playerStateMachine = new PlayerStateMachine();

        playerIdleState = new PlayerIdleState(this, playerStateMachine, "Idle");
        playerMoveState = new PlayerWalkState(this, playerStateMachine, "Move");
        playerRunState = new PlayerRunState(this, playerStateMachine, "Run");
        playerCrouchState = new PlayerCrouchState(this, playerStateMachine, "Crouch");
        playerJumpState = new PlayerJumpState(this, playerStateMachine, "Jump");
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

    /// <summary>
    /// 무기 바꿀 때 호출
    /// </summary>
    public void SetWeapon(WeaponType weaponType)
    {
        if (weaponDict.TryGetValue(weaponType, out var weapon))
        {
            currentWeapon = weapon;
        }
        else
        {
            Debug.LogError($"Weapon type {weaponType} 없음");
        }
    }
}
