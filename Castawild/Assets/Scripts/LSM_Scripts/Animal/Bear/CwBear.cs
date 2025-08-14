using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations; 

/// <summary>
/// 논플레이어블 캐릭터 클래스
/// </summary>
public class CwBear : CwAnimal
{
    // Animal 전용 필드 추가
    #region Bear Info   
    protected BearAnim bearAnim;  
    public Vector3[] layOver = new Vector3[3]; // 곰의 이동 포인트
    public int layOverIndex = 0;               // 현재 이동 포인트
    public bool IsReturned = false;
    public GameObject attackCollider;  
    #endregion

    #region Setters and Getters 
    #endregion

    public override void Initialize(CharacterData baseData)
    {
        base.Initialize(baseData);
    }

    protected override void Awake()
    {
        AddrPath = "Assets/Scriptable Objects/Bear Data.asset"; 
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
    /// 초록 : 공격 범위
    /// 빨강 : 탐지 범위
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        //공격 범위
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // [탐지범위] Gizmos 
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, maxDetectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDetectionRadius);
    }

    /// <summary>
    /// [곰 고유 기능] 3개의 순차 랜덤 NavMesh 좌표 세팅
    /// </summary>
    public void RandomNavTriangle(Vector3 origin, float minDist, float maxDist, int layermask)
    {
        layOver[0] = origin;  
        Vector3 currentOrigin = origin;
        for (int i = 1; i < layOver.Length; i++)
        {
            layOver[i] = RandomNavSphere(currentOrigin, minDist, maxDist, layermask);
            currentOrigin = layOver[i];  
        }
    }

    public void OnAttackCollider()
    {
        attackCollider.SetActive(true);
    }
    public void OffAttackCollider()
    {
        attackCollider.SetActive(false);
    }


}

