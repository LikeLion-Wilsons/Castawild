using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


/// <summary>
/// 플레이어블 캐릭터 클래스 
/// </summary>

public class CwPlayer : CwCharacter
{

    #region Player Info   
    //Player 전용 필드 추가
    #endregion

    protected override void CharacterInitialize<T>(T data)
    {
        base.CharacterInitialize<T>(data);
        //Player 전용 필드 초기화
    }

    protected override async void Start()
    {
        // Addressables를 통해 캐릭터 데이터를 비동기적으로 로드
        AsyncOperationHandle<PlayerData> handle = Addressables.LoadAssetAsync<PlayerData>(CharacterName);

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

