using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class GenericNetworkSpawner<T, U> : NetworkBehaviour
    where T : NetworkBehaviour, ISpawnable // ISpawnable interface
    where U : SpawnableDefinition // SpawnableDefinition class
{
    [Header("Definitions")]
    [SerializeField] protected List<U> definitions;

    [Header("Terrain Settings")]
    [SerializeField] protected List<TerrainSpawnSettings> terrainSettings;

    [Header("Spawn Settings")]
    [SerializeField] protected int maxSpawnAttempts = 20;
    [SerializeField] protected float checkInterval = 5f;
    [SerializeField] protected float reviveDelay = 10f;

    protected Dictionary<Terrain, float[,,]> terrainAlphaMapCache = new();
    protected Dictionary<Terrain, int> terrainTextureIndexCache = new();
    protected Dictionary<GameObject, U> prefabToDefinitionMap = new();
    protected List<GameObject> loadedPrefabs = new();
    protected int nextInstanceId = 1;

    protected class DeadObjectEntry
    {
        public T spawnableObject;
        public float reviveAtTime;
    }
    protected List<DeadObjectEntry> deadObjects = new();

    public override void Spawned()
    {
        if (HasStateAuthority)
            StartCoroutine(InitAndSpawnLoop());
    }

    protected IEnumerator InitAndSpawnLoop()
    {
        yield return CacheTerrainAlphamaps();
        yield return LoadPrefabs();

        if (loadedPrefabs.Count > 0)
            StartCoroutine(SpawnLoop());
    }

    protected IEnumerator CacheTerrainAlphamaps()
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

    protected IEnumerator LoadPrefabs()
    {
        prefabToDefinitionMap.Clear();
        loadedPrefabs.Clear();

        foreach (var def in definitions)
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

    protected IEnumerator SpawnLoop()
    {
        while (true)
        {
            foreach (var setting in terrainSettings)
            {
                int needed = setting.maxTrees - setting.activeTrees.Count;
                for (int i = 0; i < needed; i++)
                {
                    TrySpawnOne(setting);
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    protected void TrySpawnOne(TerrainSpawnSettings setting)
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

            SpawnObject(worldPos, setting);
            return;
        }
    }

    protected bool IsOnValidTexture(Vector3 pos, Terrain terrain)
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

    protected bool IsOverlapping(Vector3 pos, TerrainSpawnSettings setting)
    {
        foreach (var tree in setting.activeTrees)
        {
            if (Vector3.Distance(tree.transform.position, pos) < setting.minDistanceBetweenTrees)
                return true;
        }
        return false;
    }

    protected void SpawnObject(Vector3 pos, TerrainSpawnSettings setting)
    {
        var prefab = loadedPrefabs[Random.Range(0, loadedPrefabs.Count)];
        if (!prefabToDefinitionMap.TryGetValue(prefab, out var def)) return;

        NetworkObject netObj = Runner.Spawn(
            prefab,
            pos,
            Quaternion.Euler(0, Random.Range(0, 360), 0),
            onBeforeSpawned: (runner, obj) =>
            {
                var spawnable = obj.GetComponent<T>();
                spawnable.Init(def, nextInstanceId++);
                spawnable.OnDied += OnObjectDied;
            }
        );

        if (netObj != null)
        {
            Runner.MoveToRunnerScene(netObj.gameObject);
            if (netObj.HasStateAuthority)
            {
                setting.activeTrees.Add(netObj);
            }
        }
    }

    protected void OnObjectDied(NetworkBehaviour obj)
    {
        if (obj is T spawnable)
        {
            deadObjects.Add(new DeadObjectEntry
            {
                spawnableObject = spawnable,
                reviveAtTime = Time.time + reviveDelay
            });

            foreach (var setting in terrainSettings)
            {
                if (setting.activeTrees.Remove(obj.GetComponent<NetworkObject>()))
                {
                    break;
                }
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        for (int i = deadObjects.Count - 1; i >= 0; i--)
        {
            if (Time.time >= deadObjects[i].reviveAtTime)
            {
                var entry = deadObjects[i];
                entry.spawnableObject.Revive();

                deadObjects.RemoveAt(i);
            }
        }
    }
}

