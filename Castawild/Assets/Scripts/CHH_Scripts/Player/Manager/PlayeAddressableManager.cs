using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayeAddressableManager : MonoBehaviour
{
    [SerializeField] private AssetReference playerArmature;

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

            CwPlayer.instance.InitializeAnim();
        };
    }
}
