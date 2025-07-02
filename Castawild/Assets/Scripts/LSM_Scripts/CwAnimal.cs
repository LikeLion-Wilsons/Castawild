using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static AnimalData;

/// <summary>
/// 논플레이어블 캐릭터 클래스
/// </summary>
public class CwAnimal : CwCharacter
{

    // Animal 전용 필드 추가
    // 추후 선공몹 비선공몹으로 다시 나눌 예정
    #region Animal Info  
    [Header("Animal Info")]
    [SerializeField] private float age;               // 나이 (추후 가축 시스템 생기면 활용가능성 있음
    [SerializeField] private float size;              // 크기 (부산물 생성 시 활용 가능성 있음)
    [SerializeField] private float weight;            // 무게 (부산물 생성 시 활용 가능성 있음)
    [SerializeField] public float detectionRadius;    // 감지 거리
    [SerializeField] public float fleeThreshold;      // 체력이 몇 % 이하일 때 도망?
    [SerializeField] public float wanderInterval;     // 유휴 상태 이동 주기 
    [SerializeField] public float attackRange;        // 공격 범위
    [SerializeField] public float attackCooldown;     // 공격 쿨타임 
    [SerializeField] public SpawnType spawnType;      // 스폰 타입 (비치, 숲, 강, 산 등)
    [SerializeField] public bool isAggressive;        // 선공 여부
    [SerializeField] public bool isFleeing;           // 도망 여부
    [SerializeField] public bool canBeHarvested;     // 죽은 후 해체 가능 여부 
    #endregion 

    protected override void CharacterInitialize<T>(T data)  
    {
        base.CharacterInitialize<T>(data);
        //Animal 전용 필드 추가 
    }

    protected override async void Start()
    {
        // Addressables를 통해 캐릭터 데이터를 비동기적으로 로드
        AsyncOperationHandle<AnimalData> handle = Addressables.LoadAssetAsync < AnimalData>(CharacterName);

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            CharacterInitialize(handle.Result);
        }
        else
        {
            Debug.LogError($"[Addressables] Failed to load CharacterData for: {CharacterName}");
        }
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
}

