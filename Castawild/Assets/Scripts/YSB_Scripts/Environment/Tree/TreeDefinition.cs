using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Tree/TreeDefinition", fileName = "NewTreeDefinition")]
public class TreeDefinition : ScriptableObject
{
    public string treeName;

    [Header("Prefab (Addressables)")]
    public AssetReferenceGameObject prefabReference;

    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Drop")]
    public string dropItemID;      // 드랍할 아이템 ID
    public int dropAmount = 1;     // 드랍 개수

    [Header("Effect & Sound")]
    public string destroySFX;      // Addressable 키 ("TreeBreakSound") 나중에 설정
    public string destroyVFX;      // Addressable 키 ("TreeDustEffect") 나중에 설정
}
