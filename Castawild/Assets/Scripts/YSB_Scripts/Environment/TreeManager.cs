using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    [Header("나무 프리팹 설정")]
    [SerializeField] private List<GameObject> treePrefabs;

    [Header("크기 설정")]
    [Tooltip("나무의 폭(X, Z축) 스케일 범위")]
    [SerializeField] private Vector2 widthScaleRange = new Vector2(0.8f, 1.2f);
    [Tooltip("나무의 높이(Y축) 스케일 범위")]
    [SerializeField] private Vector2 heightScaleRange = new Vector2(0.9f, 1.1f);

    [Header("스폰 지역 설정")]
    [SerializeField] private List<BoxCollider> spawnRegions;
    [SerializeField] private int treesPerRegion = 30;
    [Tooltip("나무 사이의 최소 거리")]
    [SerializeField] private float minDistanceBetweenTrees = 4f;
    [Tooltip("스폰 위치를 찾기 위한 최대 시도 횟수")]
    [SerializeField] private int maxSpawnAttempts = 20;

    [Header("리스폰 설정")]
    [SerializeField] private float respawnTime = 60f;

    [Header("최적화 설정")]
    [Tooltip("지형 레이어")]
    [SerializeField] private LayerMask terrainLayer;
    [Tooltip("풀 사이즈")]
    [SerializeField] private int initialPoolSize = 20;

    private Dictionary<GameObject, Queue<GameObject>> treePools = new();
    private Dictionary<string, TreeSpawnPoint> spawnPoints = new();

    void Start()
    {
        if (treePrefabs == null || treePrefabs.Count == 0)
        {
            Debug.LogError("나무 프리팹이 설정되지 않았습니다!");
            return;
        }
        InitializePools();
        GenerateInitialTrees();
    }

    void InitializePools()
    {
        foreach (GameObject prefab in treePrefabs)
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
        for (int i = 0; i < spawnRegions.Count; i++)
        {
            int regionTrees = 0;
            for (int j = 0; j < treesPerRegion; j++)
            {
                if (TrySpawnSingleTree(i, j))
                {
                    regionTrees++;
                }
            }
            totalTreesSpawned += regionTrees;
        }
    }

    bool TrySpawnSingleTree(int regionIndex, int treeIndex)
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector3 randomPoint = GetRandomPointInBounds(spawnRegions[regionIndex].bounds);

            if (Physics.Raycast(randomPoint + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 200f, terrainLayer))
            {
                Vector3 spawnPosition = hit.point;

                if (!IsOverlapping(spawnPosition))
                {
                    string id = $"R{regionIndex}_T{treeIndex}";
                    GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Count)];

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
        }
        return false;
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

        // 랜덤 회전 및 크기 설정
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

    Vector3 GetRandomPointInBounds(Bounds b)
    {
        return new Vector3(
            Random.Range(b.min.x, b.max.x),
            b.center.y + b.size.y,
            Random.Range(b.min.z, b.max.z)
        );
    }
}

