using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Fusion;
using Test;

public class NetworkTreeSpawner : NetworkBehaviour
{
    [Header("Tree Definitions")]
    [SerializeField] private List<TreeDefinition> treeDefinitions;

    [Header("Terrain Settings")]
    [SerializeField] private List<TerrainSpawnSettings> terrainSettings;

    [Header("Spawn Settings")]

    [SerializeField,Tooltip("스폰 최대 횟수")] private int maxSpawnAttempts = 20;
    [SerializeField, Tooltip("나무 거리 체크 간격")] private float checkInterval = 5f;
    [SerializeField, Tooltip("revive 지연 시간")] private float reviveDelay = 10f;

    [Header("Pooling Settings")]
    [SerializeField, Tooltip("트리 프리팹 최대 풀 개수. 0이면 무한, -1이면 풀링 안 함")]
    private int maxPoolCountPerPrefab = 10;

    private Dictionary<Terrain, float[,,]> terrainAlphaMapCache = new();
    private Dictionary<Terrain, int> terrainTextureIndexCache = new();
    private Dictionary<GameObject, TreeDefinition> prefabToDefinitionMap = new();
    private List<GameObject> loadedPrefabs = new();
    private int nextTreeId = 1; // 유니크 트리 ID 부여용(임시)

    // 트리 죽음 후 리젠을 위한 구조체
    private class DeadTreeEntry
    {
        public YSB_Scripts.NetworkTree tree;
        public float reviveAtTime;
    }

    private List<DeadTreeEntry> deadTrees = new();

    public override void Spawned()
    {
        if (HasStateAuthority)
            StartCoroutine(InitAndSpawnLoop());
    }

    // 스폰 루프 초기화, 시작하는 코루틴
    private IEnumerator InitAndSpawnLoop()
    {
        yield return CacheTerrainAlphamaps(); // 알파맵 캐싱
        yield return LoadTreePrefabs();       // 프리팹 로딩

        // 풀 최대 개수 세팅
        foreach (var prefab in loadedPrefabs)
        {
            if (Runner != null && prefab != null)
            {
                Runner.SetMaxPool(prefab.name, maxPoolCountPerPrefab);
            }
        }

        if (loadedPrefabs.Count > 0)
            StartCoroutine(SpawnLoop());       // 스폰 루프 시작
    }

    // 터레인 알파맵과 텍스처 인덱스를 캐시를 위한 함수
    private IEnumerator CacheTerrainAlphamaps()
    {
        foreach (var setting in terrainSettings)
        {
            if (setting.terrain == null) continue;

            TerrainData data = setting.terrain.terrainData;
            int index = setting.spawnTextureLayerIndex;
            if (index >= data.alphamapLayers) continue;

            var alphamaps = data.GetAlphamaps(0, 0, data.alphamapWidth, data.alphamapHeight);
            terrainAlphaMapCache[setting.terrain] = alphamaps;
            terrainTextureIndexCache[setting.terrain] = index;
        }
        yield return null;
    }

    // 어드레서블 리소스에서 트리 로드하는 함수
    private IEnumerator LoadTreePrefabs()
    {
        prefabToDefinitionMap.Clear();
        loadedPrefabs.Clear();

        foreach (var def in treeDefinitions)
        {
            if (def.prefabReference.RuntimeKeyIsValid())
            {
                var handle = def.prefabReference.LoadAssetAsync<GameObject>();
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var prefab = handle.Result;
                    loadedPrefabs.Add(prefab);
                    prefabToDefinitionMap[prefab] = def;
                }
            }
        }
    }

    // 터레인의 특정 텍스처에서 최대 트리 수를 유지하며 스폰하는 코루틴
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            foreach (var setting in terrainSettings)
            {
                int needed = setting.maxTrees - setting.activeTrees.Count;
                for (int i = 0; i < needed; i++)
                {
                    TrySpawnOneTree(setting);
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    // 터레인에서 트리를 스폰하는 함수(Handler)
    // maxSpawnAttempts 이하로 시도, 텍스처가 유효한지, 나무끼리 겹치지 않는지 확인 후 스폰
    private void TrySpawnOneTree(TerrainSpawnSettings setting)
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            var terrain = setting.terrain;
            var data = terrain.terrainData;

            Vector3 worldPos = terrain.transform.position + new Vector3(
                Random.Range(0, data.size.x),
                0,
                Random.Range(0, data.size.z)
            );
            worldPos.y = terrain.SampleHeight(worldPos);

            if (worldPos.y < setting.minSpawnHeight) continue;
            if (!IsOnValidTexture(worldPos, terrain)) continue;
            if (IsOverlapping(worldPos, setting)) continue;

            SpawnTree(worldPos, setting);
            return;
        }
    }

    // 터레인에서 특정 텍스처 위에 있는지 확인하는 함수
    private bool IsOnValidTexture(Vector3 pos, Terrain terrain)
    {
        if (!terrainAlphaMapCache.TryGetValue(terrain, out var alphamaps)) return false;

        TerrainData data = terrain.terrainData;
        Vector3 localPos = pos - terrain.transform.position;

        int mapX = Mathf.FloorToInt((localPos.x / data.size.x) * data.alphamapWidth);
        int mapZ = Mathf.FloorToInt((localPos.z / data.size.z) * data.alphamapHeight);

        mapX = Mathf.Clamp(mapX, 0, data.alphamapWidth - 1);
        mapZ = Mathf.Clamp(mapZ, 0, data.alphamapHeight - 1);

        int layerIndex = terrainTextureIndexCache[terrain];
        float value = alphamaps[mapZ, mapX, layerIndex];
        return value > 0.5f;
    }

    // 터레인에서 트리가 겹치는지 확인하는 함수
    private bool IsOverlapping(Vector3 pos, TerrainSpawnSettings setting)
    {
        foreach (var tree in setting.activeTrees)
        {
            if (Vector3.Distance(tree.transform.position, pos) < setting.minDistanceBetweenTrees)
                return true;
        }
        return false;
    }

    // 텍스처, 트리 겹침 확인 후 트리 스폰하는 함수
    private void SpawnTree(Vector3 pos, TerrainSpawnSettings setting)
    {
        var prefab = loadedPrefabs[Random.Range(0, loadedPrefabs.Count)];

        if (!prefabToDefinitionMap.TryGetValue(prefab, out var def))
        {
            Debug.LogError("No TreeDefinition for prefab");
            return;
        }

        if (prefab == null || prefab.GetComponent<NetworkObject>() == null)
        {
            Debug.LogError("Prefab null or missing NetworkObject");
            return;
        }

        NetworkObject treeObj = Runner.Spawn(
            prefab,
            pos,
            Quaternion.Euler(0, Random.Range(0, 360), 0),
            inputAuthority: null,
            onBeforeSpawned: (runner, obj) =>
            {
                var tree = obj.GetComponent<YSB_Scripts.NetworkTree>();
                tree.Init(def, nextTreeId++);
                tree.OnTreeDied += OnTreeDied;
            }
        );

        // 후입자에게도 보이도록 러너 씬으로 이동
        if (treeObj != null)
        {
            Runner.MoveToRunnerScene(treeObj.gameObject);

            if (treeObj.HasStateAuthority)
            {
                setting.activeTrees.Add(treeObj);
            }
        }
    }

    // 트리가 죽었을 때 호출되는 콜백
    private void OnTreeDied(YSB_Scripts.NetworkTree tree)
    {
        // 가시성 관리자에게서 이 나무를 제거하여, 죽어있는 동안 다시 나타나지 않도록 합니다.
        NetworkObjectVisibilityManager.Instance?.UnregisterObject(tree.Object);

        deadTrees.Add(new DeadTreeEntry
        {
            tree = tree,
            reviveAtTime = Time.time + reviveDelay
        });
    }

    // FixedUpdateNetwork에서 리젠 시간 체크 및 복구
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        for (int i = deadTrees.Count - 1; i >= 0; i--)
        {
            if (Time.time >= deadTrees[i].reviveAtTime)
            {
                var entry = deadTrees[i];
                entry.tree.Revive();

                // 나무가 부활했으므로, 다시 가시성 관리 대상에 포함시킵니다.
                NetworkObjectVisibilityManager.Instance?.RegisterObject(entry.tree.Object);

                deadTrees.RemoveAt(i);
            }
        }
    }
}
