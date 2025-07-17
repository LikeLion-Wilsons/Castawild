using UnityEngine;
using UnityEngine.AddressableAssets;


public abstract class SpawnableDefinition : ScriptableObject
{
    public string resourceName;
    public AssetReferenceGameObject prefabReference;
}

