using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayeAddressableManager : MonoBehaviour
{
    [SerializeField] private AssetReference playerArmature;
    [SerializeField] private AssetReferenceT<PlayerData> playerData;

    private void Start()
    {
        Addressables.InitializeAsync().Completed += Completed;
    }

    private void Completed(AsyncOperationHandle<IResourceLocator> obj)
    {
        playerArmature.InstantiateAsync().Completed += (_playerArmature) =>
        {
            GameObject armature = _playerArmature.Result;
            armature.transform.SetParent(CwPlayer.instance.transform);
            armature.transform.localPosition = Vector3.zero;
            armature.transform.localRotation = Quaternion.identity;

            CwPlayer.instance.inputManager.RegisterAnimatorManger(armature.GetComponent<PlayerAnimatorManager>());
        };

        playerData.LoadAssetAsync().Completed += (_playerData) =>
        {
            PlayerData data = _playerData.Result;
            CwPlayer.instance.InitializeData(data);
        };
    }
}