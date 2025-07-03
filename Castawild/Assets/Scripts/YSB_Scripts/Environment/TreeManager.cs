using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PooledTree : MonoBehaviour
{
    public string spawnPointId;
}

public class TreeSpawnPoint
{
    public string id;
    public Vector3 position;
    public bool occupied;
    public GameObject prefabType;
    public GameObject treeInstance;
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

    [Header("스폰 지역 설정 (터레인)")]
    [Tooltip("나무를 스폰할 터레인 목록")]
    [SerializeField] private List<Terrain> spawnTerrains;
    [Tooltip("나무 스폰에 사용할 터레인 텍스")]
    [SerializeField] private int spawnTextureLayerIndex = 0;
    [Tooltip("스폰할 전체 나무의 수")]
    [SerializeField] private int numberOfTreesToSpawn = 300;
    [Tooltip("나무 사이의 최소 거리")]
    [SerializeField] private float minDistanceBetweenTrees = 4f;
    [Tooltip("스폰 위치를 찾기 위한 최대 시도 횟수")]
    [SerializeField] private int maxSpawnAttempts = 20;


    [Header("리스폰 설정")]
    [SerializeField] private float respawnTime = 60f;

    [Header("최적화 설정")]
    [Tooltip("풀 사이즈")]
    [SerializeField] private int initialPoolSize = 20;

    private List<GameObject> loadedTreePrefabs = new List<GameObject>();
    private Dictionary<GameObject, Queue<GameObject>> treePools = new();
    private Dictionary<string, TreeSpawnPoint> spawnPoints = new();
    
    private Dictionary<Terrain, float[,,]> terrainAlphamapCache = new();

    IEnumerator Start()
    {
        if (treePrefabReferences == null || treePrefabReferences.Count == 0)
        {
            Debug.LogError("나무 프리팹(Addressable)이 설정되지 않았습니다!");
            yield break;
        }
        if (spawnTerrains == null || spawnTerrains.Count == 0)
        {
            Debug.LogError("스폰 터레인이 설정되지 않았습니다!");
            yield break;
        }

        foreach (var terrain in spawnTerrains)
        {
            var terrainData = terrain.terrainData;
            if (spawnTextureLayerIndex >= terrainData.alphamapLayers)
            {
                Debug.LogError($"설정된 텍스처 레이어 인덱스({spawnTextureLayerIndex})가 터레인 '{terrain.name}'의 레이어 수({terrainData.alphamapLayers})보다 큽니다.");
                continue; 
            }
            var alphamapData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            terrainAlphamapCache[terrain] = alphamapData;
        }

        if (terrainAlphamapCache.Count == 0)
        {
            Debug.LogError("유효한 스폰 터레인이 없습니다. 텍스처 레이어 인덱스를 확인하세요.");
            yield break;
        }

        yield return LoadTreePrefabs();

        if (loadedTreePrefabs.Count > 0)
        {
            InitializePools();
            GenerateInitialTrees();
        }
        else
        {
            Debug.LogError("로드된 나무 프리팹이 없어 초기화를 중단합니다.");
        }
    }

    IEnumerator LoadTreePrefabs()
    {
        Debug.Log("나무 프리팹 로딩 시작...");
        var loadHandles = new List<AsyncOperationHandle<GameObject>>();
        foreach (var reference in treePrefabReferences)
        {
            if (reference.RuntimeKeyIsValid())
            {
                var handle = reference.LoadAssetAsync<GameObject>();
                loadHandles.Add(handle);
            }
            else
            {
                Debug.LogError("유효하지 않은 Addressable 키입니다.");
            }
        }

        foreach (var handle in loadHandles)
        {
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedTreePrefabs.Add(handle.Result);
            }
            else
            {
                Debug.LogError($"프리팹 로딩 실패: {handle.DebugName}");
            }
        }
        
        if(loadedTreePrefabs.Count == 0)
        {
            Debug.LogError("어떤 나무 프리팹도 로드하지 못했습니다. Addressable 설정을 확인하세요.");
        }
        else
        {
            Debug.Log($"{loadedTreePrefabs.Count}개의 나무 프리팹 로딩 완료.");
        }
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

    void GenerateInitialTrees()
    {
        int totalTreesSpawned = 0;
        for (int i = 0; i < numberOfTreesToSpawn; i++)
        {
            if (TrySpawnSingleTree(i))
            {
                totalTreesSpawned++;
            }
        }
        Debug.Log($"{totalTreesSpawned}개의 나무가 생성되었습니다.");
    }

    bool TrySpawnSingleTree(int treeIndex)
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Terrain selectedTerrain = new List<Terrain>(terrainAlphamapCache.Keys)[Random.Range(0, terrainAlphamapCache.Count)];
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

            if (!IsOverlapping(spawnPosition))
            {
                string id = $"T_{treeIndex}";
                GameObject prefab = loadedTreePrefabs[Random.Range(0, loadedTreePrefabs.Count)];

                TreeSpawnPoint sp = new TreeSpawnPoint
                {
                    id = id,
                    position = spawnPosition,
                    occupied = true,
                    prefabType = prefab
                };
                spawnPoints[id] = sp;

                SpawnTreeFromPool(sp);
                return true;
            }
        }
        return false;
    }
    
    bool IsOnCorrectTexture(Vector3 worldPosition, Terrain terrain)
    {
        if (!terrainAlphamapCache.ContainsKey(terrain)) return false;

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = terrain.transform.position;
        float[,,] alphamapData = terrainAlphamapCache[terrain];

        Vector3 relativePos = worldPosition - terrainPosition;
        float normX = relativePos.x / terrainData.size.x;
        float normZ = relativePos.z / terrainData.size.z;

        int mapX = (int)(normX * terrainData.alphamapWidth);
        int mapZ = (int)(normZ * terrainData.alphamapHeight);

        if (mapX < 0 || mapX >= terrainData.alphamapWidth || mapZ < 0 || mapZ >= terrainData.alphamapHeight)
        {
            return false;
        }

        float textureValue = alphamapData[mapZ, mapX, spawnTextureLayerIndex];

        return textureValue > 0.5f;
    }

    bool IsOverlapping(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, minDistanceBetweenTrees);
        foreach (var collider in colliders)
        {
            if (collider.GetComponentInParent<PooledTree>() != null)
            {
                return true;
            }
        }
        return false;
    }

    void SpawnTreeFromPool(TreeSpawnPoint point)
    {
        Queue<GameObject> pool = treePools[point.prefabType];
        GameObject tree;

        if (pool.Count > 0)
        {
            tree = pool.Dequeue();
        }
        else
        {
            tree = Instantiate(point.prefabType, transform);
        }

        tree.transform.position = point.position;

        tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        float randomWidth = Random.Range(widthScaleRange.x, widthScaleRange.y);
        float randomHeight = Random.Range(heightScaleRange.x, heightScaleRange.y);
        tree.transform.localScale = new Vector3(randomWidth, randomHeight, randomWidth);

        tree.SetActive(true);

        point.treeInstance = tree;

        TreeHealth health = tree.GetComponent<TreeHealth>() ?? tree.AddComponent<TreeHealth>();
        health.Init(point.id, OnTreeDestroyed);

        PooledTree pooledTree = tree.GetComponent<PooledTree>() ?? tree.AddComponent<PooledTree>();
        pooledTree.spawnPointId = point.id;
    }

    public void OnTreeDestroyed(string id)
    {
        if (spawnPoints.TryGetValue(id, out var point))
        {
            point.occupied = false;
            if (point.treeInstance != null)
            {
                ReturnTreeToPool(point);
            }
            StartCoroutine(RespawnCoroutine(id));
        }
    }

    void ReturnTreeToPool(TreeSpawnPoint point)
    {
        if (point.treeInstance != null)
        {
            point.treeInstance.SetActive(false);
            treePools[point.prefabType].Enqueue(point.treeInstance);
            point.treeInstance = null;
        }
    }

    IEnumerator RespawnCoroutine(string id)
    {
        yield return new WaitForSeconds(respawnTime);
        if (spawnPoints.TryGetValue(id, out var point))
        {
            point.occupied = true;
            SpawnTreeFromPool(point);
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

