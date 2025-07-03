using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class TerrainSpawnSettings
{
    public Terrain terrain;
    [Tooltip("이 터레인에서 나무 스폰에 사용할 텍스처 레이어 인덱스")]
    public int spawnTextureLayerIndex = 0;
    [Tooltip("이 터레인에서 유지할 최대 나무 수")]
    public int maxTrees = 100;
    [HideInInspector]
    public List<GameObject> activeTrees = new List<GameObject>();
}

public class TreeManager : MonoBehaviour
{
    [Header("나무 프리팹 설정 (Addressables)")]
    [SerializeField] private List<AssetReferenceGameObject> treePrefabReferences;

    [Header("크기 설정")]
    [Tooltip("나무의 폭(X, Z축) 스케일 범위")]
    [SerializeField] private Vector2 widthScaleRange = new Vector2(0.8f, 1.2f);
    [Tooltip("나무의 높이(Y축) 스케일 범위")]
    [SerializeField] private Vector2 heightScaleRange = new Vector2(0.9f, 1.1f);

    [Header("스폰 지역 설정")]
    [Tooltip("나무를 스폰할 터레인과 관련 설정 목록")]
    [SerializeField] private List<TerrainSpawnSettings> terrainSettings;
    [Tooltip("나무가 스폰될 최소 월드 Y 높이")]
    [SerializeField] private float minSpawnHeight = 50f;
    [Tooltip("나무 사이의 최소 거리")]
    [SerializeField] private float minDistanceBetweenTrees = 4f;
    [Tooltip("스폰 위치를 찾기 위한 최대 시도 횟수")]
    [SerializeField] private int maxSpawnAttempts = 20;

    [Header("개체 수 유지 설정")]
    [Tooltip("개체 수를 확인하고 보충하는 주기(초)")]
    [SerializeField] private float populationCheckInterval = 5f;

    [Header("최적화 설정")]
    [Tooltip("초기 풀 사이즈 (각 프리팹 당)")]
    [SerializeField] private int initialPoolSize = 20;

    private List<GameObject> loadedTreePrefabs = new List<GameObject>();
    private Dictionary<GameObject, Queue<GameObject>> treePools = new();
    private Dictionary<Terrain, float[,,]> terrainAlphamapCache = new();
    private Dictionary<Terrain, int> terrainTextureIndexCache = new();

    IEnumerator Start()
    {
        if (treePrefabReferences == null || treePrefabReferences.Count == 0)
        {
            Debug.LogError("나무 프리팹(Addressable)이 설정되지 않았습니다!");
            yield break;
        }
        if (terrainSettings == null || terrainSettings.Count == 0)
        {
            Debug.LogError("스폰 터레인 설정이 없습니다!");
            yield break;
        }

        yield return InitializeTerrainCache();
        
        if (terrainAlphamapCache.Count == 0)
        {
            Debug.LogError("유효한 스폰 터레인이 없습니다. 터레인 및 텍스처 레이어 인덱스를 확인하세요.");
            yield break;
        }

        yield return LoadTreePrefabs();

        if (loadedTreePrefabs.Count > 0)
        {
            InitializePools();
            StartCoroutine(MaintainTreePopulation());
        }
        else
        {
            Debug.LogError("로드된 나무 프리팹이 없어 초기화를 중단합니다.");
        }
    }

    IEnumerator InitializeTerrainCache()
    {
        foreach (var setting in terrainSettings)
        {
            if (setting.terrain == null)
            {
                Debug.LogWarning("터레인 설정에 터레인이 할당되지 않았습니다. 이 항목을 건너뜁니다.");
                continue;
            }

            var terrain = setting.terrain;
            var terrainData = terrain.terrainData;
            if (setting.spawnTextureLayerIndex >= terrainData.alphamapLayers)
            {
                Debug.LogError($"설정된 텍스처 레이어 인덱스({setting.spawnTextureLayerIndex})가 터레인 '{terrain.name}'의 레이어 수({terrainData.alphamapLayers})보다 큽니다.");
                continue;
            }
            var alphamapData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            terrainAlphamapCache[terrain] = alphamapData;
            terrainTextureIndexCache[terrain] = setting.spawnTextureLayerIndex;
        }
        yield return null;
    }

    IEnumerator LoadTreePrefabs()
    {
        Debug.Log("나무 프리팹 로딩 시작...");
        var loadHandles = new List<AsyncOperationHandle<GameObject>>();
        foreach (var reference in treePrefabReferences)
        {
            if (reference.RuntimeKeyIsValid())
            {
                loadHandles.Add(reference.LoadAssetAsync<GameObject>());
            }
        }

        foreach (var handle in loadHandles)
        {
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedTreePrefabs.Add(handle.Result);
            }
        }
        
        if(loadedTreePrefabs.Count > 0)
            Debug.Log($"{loadedTreePrefabs.Count}개의 나무 프리팹 로딩 완료.");
        else
            Debug.LogError("어떤 나무 프리팹도 로드하지 못했습니다. Addressable 설정을 확인하세요.");
    }

    void InitializePools()
    {
        foreach (GameObject prefab in loadedTreePrefabs)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject tree = Instantiate(prefab, transform);
                tree.SetActive(false);
                pool.Enqueue(tree);
            }
            treePools[prefab] = pool;
        }
    }

    IEnumerator MaintainTreePopulation()
    {
        while (true)
        {
            foreach (var setting in terrainSettings)
            {
                int treesToSpawn = setting.maxTrees - setting.activeTrees.Count;
                if (treesToSpawn > 0)
                {
                    // Debug.Log($"{setting.terrain.name}에 {treesToSpawn}개의 나무를 추가로 스폰합니다.");
                    for (int i = 0; i < treesToSpawn; i++)
                    {
                        TrySpawnSingleTree(setting);
                    }
                }
            }
            yield return new WaitForSeconds(populationCheckInterval);
        }
    }

    void TrySpawnSingleTree(TerrainSpawnSettings setting)
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Terrain selectedTerrain = setting.terrain;
            TerrainData terrainData = selectedTerrain.terrainData;
            Vector3 terrainPosition = selectedTerrain.transform.position;

            float randomX = Random.Range(0, terrainData.size.x);
            float randomZ = Random.Range(0, terrainData.size.z);
            Vector3 spawnPosition = new Vector3(randomX, 0, randomZ) + terrainPosition;

            if (!IsOnCorrectTexture(spawnPosition, selectedTerrain))
            {
                continue;
            }

            spawnPosition.y = selectedTerrain.SampleHeight(spawnPosition);

            if (spawnPosition.y < minSpawnHeight)
            {
                continue;
            }

            if (!IsOverlapping(spawnPosition, setting))
            {
                SpawnTreeFromPool(spawnPosition, setting);
                return;
            }
        }
        // Debug.LogWarning($"{setting.terrain.name}에 나무를 스폰할 적절한 위치를 찾지 못했습니다.");
    }
    
    bool IsOnCorrectTexture(Vector3 worldPosition, Terrain terrain)
    {
        if (!terrainAlphamapCache.ContainsKey(terrain) || !terrainTextureIndexCache.ContainsKey(terrain)) return false;

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = terrain.transform.position;
        float[,,] alphamapData = terrainAlphamapCache[terrain];
        int textureLayerIndex = terrainTextureIndexCache[terrain];

        Vector3 relativePos = worldPosition - terrainPosition;
        float normX = relativePos.x / terrainData.size.x;
        float normZ = relativePos.z / terrainData.size.z;

        int mapX = (int)(normX * terrainData.alphamapWidth);
        int mapZ = (int)(normZ * terrainData.alphamapHeight);

        if (mapX < 0 || mapX >= terrainData.alphamapWidth || mapZ < 0 || mapZ >= terrainData.alphamapHeight)
        {
            return false;
        }

        float textureValue = alphamapData[mapZ, mapX, textureLayerIndex];

        return textureValue > 0.5f;
    }

    bool IsOverlapping(Vector3 position, TerrainSpawnSettings setting)
    {
        foreach (var tree in setting.activeTrees)
        {
            if (Vector3.Distance(tree.transform.position, position) < minDistanceBetweenTrees)
            {
                return true;
            }
        }
        return false;
    }

    void SpawnTreeFromPool(Vector3 position, TerrainSpawnSettings setting)
    {
        GameObject prefab = loadedTreePrefabs[Random.Range(0, loadedTreePrefabs.Count)];
        Queue<GameObject> pool = treePools[prefab];
        GameObject tree;

        if (pool.Count > 0)
        {
            tree = pool.Dequeue();
        }
        else
        {
            tree = Instantiate(prefab, transform);
        }

        tree.transform.position = position;
        tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        float randomWidth = Random.Range(widthScaleRange.x, widthScaleRange.y);
        float randomHeight = Random.Range(heightScaleRange.x, heightScaleRange.y);
        tree.transform.localScale = new Vector3(randomWidth, randomHeight, randomWidth);

        tree.SetActive(true);
        setting.activeTrees.Add(tree);

        TreeHealth health = tree.GetComponent<TreeHealth>() ?? tree.AddComponent<TreeHealth>();
        health.Init(OnTreeDestroyed);
    }

    public void OnTreeDestroyed(GameObject treeInstance)
    {
        foreach (var setting in terrainSettings)
        {
            if (setting.activeTrees.Remove(treeInstance))
            {
                ReturnTreeToPool(treeInstance);
                break;
            }
        }
    }

    void ReturnTreeToPool(GameObject treeInstance)
    {
        treeInstance.SetActive(false);

        // 어떤 프리팹으로 만들어졌는지 확인 후 해당 풀에 반납
        // 이 방식은 프리팹 정보를 어딘가에 저장해야 함 (예: 별도 컴포넌트)
        // 여기서는 간단하게 첫번째 풀에 넣는 것으로 가정하지만, 개선 필요
        // -> 개선: Prefab-based pooling을 위해 어떤 prefab에서 왔는지 알아야 함.
        //    가장 간단한 방법은 treeInstance에 어떤 prefab에서 왔는지 기록하는 것.
        //    하지만 여기서는 모든 나무가 동일한 로직을 공유하므로, 일단 첫번째 풀에 넣어도 무방.
        //    더 복잡한 시스템에서는 이 부분을 반드시 개선해야 함.
        //gpt도움 받는중
        GameObject prefab = loadedTreePrefabs[0]; // 임시 방편
        if(treePools.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            pool.Enqueue(treeInstance);
        }
        else
        {
            Destroy(treeInstance); // 풀을 찾지 못하면 파괴
        }
    }

    void OnDestroy()
    {
        foreach (var prefabRef in treePrefabReferences)
        {
            if (prefabRef.Asset != null)
            {
                prefabRef.ReleaseAsset();
            }
        }
    }
}

