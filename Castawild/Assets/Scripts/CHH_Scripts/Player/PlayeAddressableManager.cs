using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayeAddressableManager : MonoBehaviour
{
    [SerializeField] private Transform playerPos;
    [SerializeField] private AssetReference playerPrefab;
    [SerializeField] private AssetReferenceT<PlayerData> playerData;

    private void Start()
    {
        Addressables.InitializeAsync().Completed += Completed;
    }

    private void Completed(AsyncOperationHandle<IResourceLocator> obj)
    {
        playerPrefab.InstantiateAsync().Completed += (_playerPrefab) =>
        {
            GameObject player = _playerPrefab.Result;
            player.transform.position = playerPos.position;
            player.transform.rotation = playerPos.rotation;

            CwPlayer playerScript = player.GetComponent<CwPlayer>();

            playerData.LoadAssetAsync().Completed += (_playerData) =>
            {
                PlayerData data = _playerData.Result;
                playerScript.InitializeData(data); // ✅ 직접 참조해서 호출
            };
        };
    }
}