using Fusion;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 캐릭터 추상 클래스
/// </summary>
public abstract class CwCharacter : NetworkBehaviour
{ 
    #region Character Info  
    [Header("Character Info")]
    [SerializeField] protected string characterName; //캐릭터 식별자
    [SerializeField] protected float maxHp; //최대체력
    [SerializeField] protected float armor; //방어력
    [SerializeField] protected float attack; //공격력 
    [SerializeField] protected float moveSpeed; //이동속도 
    [SerializeField] protected string AddrPath;

    [Header("Network Info")]
    [Networked][SerializeField]
    protected float currentHp { get; set; } //현재 체력
    #endregion

    #region Setters and Getters

    protected virtual string CharacterName
    {
        get => characterName;
        set => characterName = value;
    }
    ///<summary>
    ///최대체력 설정 및 반환 함수
    ///</summary>
    protected virtual float MaxHp
    {
        get => maxHp;
        set
        {
            maxHp = value;
            if (currentHp > maxHp)
                currentHp = value;
        }
    }

    ///<summary>
    ///현재체력 반환 함수
    ///</summary>
    public virtual float CurrentHp
    {
        get => currentHp;
        set => currentHp = value;
    }

    ///<summary>
    ///방어력 설정 및 반환 함수
    ///</summary>
    public virtual float Armor
    {
        get => armor;
        set => armor = value;
    }

    ///<summary>
    ///공격력 설정 및 반환 함수
    ///</summary>
    public virtual float Attack
    {
        get => attack;
        set => attack = value;
    }
    ///<summary>
    ///이동속도 설정 및 반환 함수
    ///</summary>
    public virtual float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }
    #endregion

    /// <summary>    
    /// 최초 생성시 데이터 동기화 함수 
    /// </summary>   
    public virtual void Initialize(CharacterData data)
    {
        CharacterName = data.characterName;
        MaxHp = data.maxHp;
        Armor = data.armor;
        Attack = data.attack;
        MoveSpeed = data.moveSpeed;

        //호스트면 현재 체력을 최대 체력으로
        if(Object.HasStateAuthority)
        {
            CurrentHp = MaxHp;
        } 
    } 
    protected virtual void Awake()
    { 
        AddrPath = "Assets/Scriptable Objects/Default Character Data.asset";
    } 
    protected virtual async void Start()
    {
        // Addressables를 통해 캐릭터 데이터를 비동기적으로 로드
        AsyncOperationHandle<CharacterData> handle = Addressables.LoadAssetAsync<CharacterData>(AddrPath);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Initialize(handle.Result);

            // 로딩 후 Addressables 해제
            Addressables.Release(handle);
        } 
    }

    /// <summary>    
    /// 피격 함수
    /// 호스트는 바로 계산
    /// 클라이언트는 RPC를 통해 State Authority에게 데미지 요청
    /// </summary>
    /// <param name="damage"> 공격 주체가 주는 최종 데미지 </param>   
    public virtual void TakeDamage(float damage)
    {
        if (Object.HasStateAuthority)
        {
            ApplyDamage(damage);
        }
        else
        {
            RPC_TakeDamage(damage);
        }
    } 

    /// <summary>    
    /// State Authority에게 데미지 요청 후 동기화 하는 함수
    /// </summary>
    /// <param name="damage"> 공격 주체가 주는 최종 데미지 </param>   
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    protected virtual void RPC_TakeDamage(float damage)
    {
        TakeDamage(damage);
    }

    protected void ApplyDamage(float damage)
    {
        CurrentHp -= (float)((Mathf.Pow(damage, 2f) / ((double)Armor + (double)damage)));

        //체력이 0 이하면 Die() 호출
        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            Die();
        } 
    }

    /// <summary>    
    /// 해당 캐릭터가 죽었을 때 호출되는 메서드
    /// </summary>
    protected abstract void Die();


    /// <summary>    
    /// 상태이상 효과 메서드
    /// </summary>
    protected abstract void StatusEffect();
}
