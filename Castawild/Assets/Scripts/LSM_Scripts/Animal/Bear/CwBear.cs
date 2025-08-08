using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations; 

/// <summary>
/// 논플레이어블 캐릭터 클래스
/// </summary>
public class CwBear : CwAnimal
{
    // Animal 전용 필드 추가
    #region Bear Info   
    protected BearAnim bearAnim; // 곰 애니메이션 클래스 
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
}

