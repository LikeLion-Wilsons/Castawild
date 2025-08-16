using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations; 

/// <summary>
/// 논플레이어블 캐릭터 클래스
/// </summary>
public class CwRabbit : CwAnimal
{
    // Animal 전용 필드 추가
    // 추후 선공몹 비선공몹으로 다시 나눌 예정
    #region Rabbit Info  
    protected Vector3 homeCenter;
    protected float homeRadius = 5f; // 집 영역 반경 
    protected RabbitAnim rabbitAnim; // 토끼 애니메이션 클래스
    #endregion

    #region Setters and Getters
    public Vector3 HomeCenter => homeCenter;
    public float HomeRadius => homeRadius;
    #endregion

    public override void Initialize(CharacterData baseData)
    {
        base.Initialize(baseData);
    }

    protected override void Awake()
    {
        AddrPath = "Assets/Scriptable Objects/Rabbit Data.asset";
        rabbitAnim = GetComponent<RabbitAnim>();
    }

    public override void Init()
    {
        base.Init();
        if (homeCenter != null)
            homeCenter = transform.position;
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
        Debug.Log($"토끼가 {_damage}의 피해를 받았습니다. 현재 체력: {CurrentHp}");         
    }

    protected override void Die()
    {
        rabbitAnim.ChangeRabbitState(RabbitAnim.RabbitState.Death);
    }

    protected override void StatusEffect()
    {

    }
}

