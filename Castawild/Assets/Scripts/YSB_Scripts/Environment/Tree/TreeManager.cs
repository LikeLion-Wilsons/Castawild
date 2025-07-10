using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TreeManager : MonoBehaviour
{
    [Header("나무 정의 설정 (ScriptableObject)")]
    [SerializeField] private List<TreeDefinition> treeDefinitions;

    [Header("크기 설정")]
    [SerializeField] private Vector2 widthScaleRange = new(0.8f, 1.2f);
    [SerializeField] private Vector2 heightScaleRange = new(0.9f, 1.1f);

    [Header("스폰 지역 설정")]
    [SerializeField] private List<TerrainSpawnSettings> terrainSettings;
    [SerializeField] private float minSpawnHeight = 50f;
    [SerializeField] private float minDistanceBetweenTrees = 4f;
    [SerializeField] private int maxSpawnAttempts = 20;

    [Header("개체 수 유지 설정")]
    [SerializeField] private float populationCheckInterval = 5f;

    [Header("최적화 설정")]
    [SerializeField] private int initialPoolSize = 20;

    private List<GameObject> loadedTreePrefabs = new();
    private Dictionary<GameObject, Queue<GameObject>> treePools = new();
    private Dictionary<GameObject, TreeDefinition> prefabToDefinitionMap = new();
    private Dictionary<Terrain, float[,,]> terrainAlphamapCache = new();
    private Dictionary<Terrain, int> terrainTextureIndexCache = new();

    private IEnumerator Start()
    {
        yield return InitializeTerrainCache();
        yield return LoadTreePrefabs();

        if (loadedTreePrefabs.Count > 0)
        {
            InitializePools();
            StartCoroutine(MaintainTreePopulation());
        }
    }

    private IEnumerator InitializeTerrainCache()
    {
        foreach (var setting in terrainSettings)
        {
            if (setting.terrain == null) continue;

            var terrainData = setting.terrain.terrainData;
            if (setting.spawnTextureLayerIndex >= terrainData.alphamapLayers) continue;

            var alphamap = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            terrainAlphamapCache[setting.terrain] = alphamap;
            terrainTextureIndexCache[setting.terrain] = setting.spawnTextureLayerIndex;
        }
        yield return null;
    }

    private IEnumerator LoadTreePrefabs()
    {
        var loadHandles = new List<AsyncOperationHandle<GameObject>>();

        foreach (var def in treeDefinitions)
        {
            if (def.prefabReference.RuntimeKeyIsValid())
                loadHandles.Add(def.prefabReference.LoadAssetAsync<GameObject>());
        }

        foreach (var handle in loadHandles)
        {
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var prefab = handle.Result;
                loadedTreePrefabs.Add(prefab);
                prefabToDefinitionMap[prefab] = treeDefinitions[loadHandles.IndexOf(handle)];
            }
        }
    }

    private void InitializePools()
    {
        foreach (var prefab in loadedTreePrefabs)
        {
            var pool = new Queue<GameObject>();
            for (int i = 0; i < initialPoolSize; i++)
            {
                var tree = Instantiate(prefab, transform);
                tree.SetActive(false);
                pool.Enqueue(tree);
            }
            treePools[prefab] = pool;
        }
    }

    private IEnumerator MaintainTreePopulation()
    {
        while (true)
        {
            foreach (var setting in terrainSettings)
            {
                int needed = setting.maxTrees - setting.activeTrees.Count;
                for (int i = 0; i < needed; i++)
                    TrySpawnSingleTree(setting);
            }
            yield return new WaitForSeconds(populationCheckInterval);
        }
    }

    private void TrySpawnSingleTree(TerrainSpawnSettings setting)
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Terrain t = setting.terrain;
            var data = t.terrainData;
            var pos = t.transform.position + new Vector3(
                UnityEngine.Random.Range(0, data.size.x),
                0,
                UnityEngine.Random.Range(0, data.size.z)
            );
            pos.y = t.SampleHeight(pos);

            if (pos.y < minSpawnHeight || !IsOnCorrectTexture(pos, t) || IsOverlapping(pos, setting))
                continue;

            SpawnTreeFromPool(pos, setting);
            return;
        }
    }

    private bool IsOnCorrectTexture(Vector3 worldPos, Terrain terrain)
    {
        if (!terrainAlphamapCache.ContainsKey(terrain)) return false;

        var data = terrain.terrainData;
        var pos = worldPos - terrain.transform.position;
        int mapX = (int)((pos.x / data.size.x) * data.alphamapWidth);
        int mapZ = (int)((pos.z / data.size.z) * data.alphamapHeight);

        float value = terrainAlphamapCache[terrain][mapZ, mapX, terrainTextureIndexCache[terrain]];
        return value > 0.5f;
    }

    private bool IsOverlapping(Vector3 pos, TerrainSpawnSettings setting)
    {
        foreach (var tree in setting.activeTrees)
        {
            if (Vector3.Distance(tree.transform.position, pos) < minDistanceBetweenTrees)
                return true;
        }
        return false;
    }

    private void SpawnTreeFromPool(Vector3 position, TerrainSpawnSettings setting)
    {
        GameObject prefab = loadedTreePrefabs[UnityEngine.Random.Range(0, loadedTreePrefabs.Count)];
        Queue<GameObject> pool = treePools[prefab];
        GameObject tree = (pool.Count > 0) ? pool.Dequeue() : Instantiate(prefab, transform);

        tree.transform.position = position;
        tree.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
        float w = UnityEngine.Random.Range(widthScaleRange.x, widthScaleRange.y);
        float h = UnityEngine.Random.Range(heightScaleRange.x, heightScaleRange.y);
        tree.transform.localScale = new Vector3(w, h, w);

        tree.SetActive(true);
        setting.activeTrees.Add(tree);

        var def = prefabToDefinitionMap[prefab];
        var health = tree.GetComponent<TreeHealth>();
        var id = Guid.NewGuid().ToString();
        health.Init(id, def, OnTreeDestroyed);

        var source = tree.GetComponent<TreeSource>() ?? tree.AddComponent<TreeSource>();
        source.prefabOrigin = prefab;
    }

    private void OnTreeDestroyed(TreeHealth health)
    {
        GameObject treeInstance = health.gameObject;
        health.ResetState();

        foreach (var setting in terrainSettings)
        {
            if (setting.activeTrees.Remove(treeInstance))
            {
                ReturnTreeToPool(treeInstance);
                break;
            }
        }
    }

    private void ReturnTreeToPool(GameObject tree)
    {
        tree.SetActive(false);
        var source = tree.GetComponent<TreeSource>();

        if (source != null && treePools.TryGetValue(source.prefabOrigin, out var pool))
        {
            pool.Enqueue(tree);
        }
        else
        {
            Destroy(tree);
        }
    }

    private void OnDestroy()
    {
        foreach (var def in treeDefinitions)
        {
            if (def.prefabReference.Asset != null)
                def.prefabReference.ReleaseAsset();
        }
    }
}