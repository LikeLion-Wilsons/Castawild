using Fusion;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;
using static AnimalData;

/// <summary>
/// 논플레이어블 캐릭터 클래스
/// </summary>
public class CwAnimal : CwCharacter
{
    // Animal 전용 필드 추가 
    #region Animal Info  
    [Header("Animal Info")]
    [SerializeField] protected SpawnType spawnType;
    [SerializeField] protected float maxDetectionRadius;
    [SerializeField] protected float minDetectionRadius;
    [SerializeField] protected float attackRange;
    [SerializeField] protected bool canBeHarvested;
    [SerializeField] protected float idleRadius;
    [SerializeField] protected float escapeRadius;
    [SerializeField] protected float escapeSpeed;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected LayerMask obstacleLayer;

    [Header("Network Info")]
    [Networked][SerializeField] protected float alertTime { set; get; }
    [Networked][SerializeField] protected float attackCooldown { set; get; }
    [Networked][SerializeField] public bool IsDead { set; get; } // 동물 사망 여부

    public Collider AnimalBody; // 동물 몸
    public GameObject AnimalCopse; // 동물 시체 

    #endregion

    #region// Setters and Getters
    public SpawnType SpawnType
    {
        get => spawnType;
        set => spawnType = value;
    }

    public float MaxDetectionRadius
    {
        get => maxDetectionRadius;
        set => maxDetectionRadius = value;
    }

    public float MinDetectionRadius
    {
        get => minDetectionRadius;
        set => minDetectionRadius = value;
    }

    public float AttackRange
    {
        get => attackRange;
        set => attackRange = value;
    }

    public float AttackCooldown
    {
        get => attackCooldown;
        set => attackCooldown = value;
    }

    public bool CanBeHarvested
    {
        get => canBeHarvested;
        set => canBeHarvested = value;
    }

    public float AlertTime
    {
        get => alertTime;
        set => alertTime = value;
    }

    public float IdleRadius
    {
        get => idleRadius;
        set => idleRadius = value;
    }

    public float EscapeRadius
    {
        get => escapeRadius;
        set => escapeRadius = value;
    }

    public float EscapeSpeed
    {
        get => escapeSpeed;
        set => escapeSpeed = value;
    }

    public LayerMask PlayerLayer
    {
        get => playerLayer;
        set => playerLayer = value;
    }

    public LayerMask ObstacleLayer
    {
        get => obstacleLayer;
        set => obstacleLayer = value;
    }
    #endregion

    public override void Initialize(CharacterData baseData)
    {
        base.Initialize(baseData);

        if (baseData is AnimalData data)
        {
            SpawnType = data.spawnType;
            MaxDetectionRadius = data.maxDetectionRadius;
            MinDetectionRadius = data.minDetectionRadius;
            AttackRange = data.attackRange;
            CanBeHarvested = data.canBeHarvested;
            IdleRadius = data.idleRadius;
            EscapeRadius = data.escapeRadius;
            EscapeSpeed = data.escapeSpeed;
           
            if (Object.HasStateAuthority)
            {
                AlertTime = data.alertTime;
                AttackCooldown = data.attackCooldown;
            }
        }
        else
        {
            Debug.LogWarning($"잘못된 데이터 타입: {baseData.GetType().Name}, AnimalData 필요");
        }
    }

    public override void Init()
    {
        base.Init();
    }

    protected override void Awake()
    {
        AddrPath = "Assets/Scriptable Objects/Default Animal Data.asset";
        if (AnimalCopse != null)
            AnimalCopse.SetActive(false);
    }

    protected override async void Start()
    {
        // Addressables를 통해 캐릭터 데이터를 비동기적으로 로드
        AsyncOperationHandle<AnimalData> handle = Addressables.LoadAssetAsync<AnimalData>(AddrPath);

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Initialize(handle.Result);
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
    /// <summary>    
    /// [동물 공통 기능]
    /// 플레이어가 감지되면 거리를 반환하고, 감지되지 않으면 null 반환
    /// </summary>
    public virtual float? IsPlayerDetection()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, maxDetectionRadius, playerLayer);

        foreach (Collider target in targets)
        {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
             
            if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayer))
            {
                return distanceToTarget;
            }
        } 
        return null;
    }

    /// <summary>
    /// [동물 공통 기능]
    /// 플레이어가 감지되면 해당 위치를 반환하고, 감지되지 않으면 null 반환
    /// </summary>
    public virtual Vector3 GetPlayerPosition()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, maxDetectionRadius, playerLayer);

        foreach (Collider target in targets)
        {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayer))
            { 
                return target.transform.position;
            }
        }
        return Vector3.zero; // 플레이어가 감지되지 않으면 제자리 반환
    }

    /// <summary>
    /// 감지 범위 표시
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // [탐지범위] Gizmos 
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, maxDetectionRadius);
         
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDetectionRadius);

        // [최소 이동 범위] Gizmos
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, minDetectionRadius);

        // [최대 이동 범위] Gizmos
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, minDetectionRadius);
    }

    /// <summary>
    /// [동물 공통 기능] 랜덤 위치 이동
    /// </summary>
    public virtual Vector3 RandomNavSphere(Vector3 origin, float minDist, float maxDist, int layermask)
    {
        Vector3 randomDirection = Random.insideUnitSphere.normalized * Random.Range(minDist, maxDist); 
        Vector3 targetPos = origin + randomDirection; 
        NavMeshHit navHit;

        if (NavMesh.SamplePosition(targetPos, out navHit, maxDist, layermask))
            return navHit.position;
        else
            return origin; // 실패 시 제자리
    }
}