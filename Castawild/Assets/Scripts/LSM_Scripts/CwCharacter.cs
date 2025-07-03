using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 캐릭터 추상 클래스,
/// 모든 캐릭터는 이 클래스를 상속받아야 함 
/// </summary>
public abstract class CwCharacter : MonoBehaviour
{
    #region Character Info  
    [Header("Character Info")]
    [SerializeField] private string characterName; //캐릭터 식별자
    [SerializeField] private float maxHp; //최대체력
    [SerializeField] private float currentHp; //현재 체력
    [SerializeField] private float armor; //방어력
    [SerializeField] private float attack; //공격력 
    [SerializeField] private float moveSpeed; //이동속도 
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
    protected virtual float Armor
    {
        get => armor;
        set => armor = value;
    }

    ///<summary>
    ///공격력 설정 및 반환 함수
    ///</summary>
    protected virtual float Attack
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
    /// <typeparam name="T">스크립터블 오브젝트 타입</typeparam>
    /// <param name="data"> 참조받을 스크립터블 오브젝트 </param>    
    protected virtual void CharacterInitialize<T>(T data) where T : CharacterData
    {
        CharacterName = data.characterName;
        MaxHp = data.maxHp;
        Armor = data.armor;
        Attack = data.attack;
        MoveSpeed = data.moveSpeed;
    }

    // 오버로드: 자식 ScriptableObject 타입도 지원
    protected virtual void CharacterInitialize<TBase, TDerived>(TDerived data)
        where TBase : CharacterData
        where TDerived : TBase
    {
        CharacterInitialize<TBase>(data); // 기본 CharacterData 필드 초기화

        // 자식 데이터의 추가 필드가 있다면 여기서 처리 (예시)
        // var derivedData = data as TDerived;
        // if (derivedData != null) { ... }
    }
    protected virtual async void Start()
    {
        // Addressables를 통해 캐릭터 데이터를 비동기적으로 로드
        AsyncOperationHandle<CharacterData> handle = Addressables.LoadAssetAsync<CharacterData>(CharacterName);

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            CharacterInitialize(handle.Result);

            // 로딩 후 Addressables 해제
            Addressables.Release(handle);
        }
        else
        {
            Debug.LogError($"[Addressables] Failed to load CharacterData for: {CharacterName}");
        }
    }

    /// <summary>    
    /// 피격 함수
    /// </summary>
    /// <param name="_damage"> 공격 주체가 주는 최종 데미지 </param>   
    public virtual void TakeDamage(float _damage)
    {
        CurrentHp -= (float)((Mathf.Pow(_damage, 2f) / ((double)Armor + (double)_damage)));

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
