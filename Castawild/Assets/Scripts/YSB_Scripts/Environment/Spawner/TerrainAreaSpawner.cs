using System.Collections;
using UnityEngine;
using Test;
using System.Collections.Generic;

// 자식
public class TerrainAreaSpawner : EnvironmentSpawner<EnvironmentObject, SpawnableDefinition>
{
    [Header("Terrain Settings")]
    [SerializeField] private List<TerrainSpawnSettings> terrainSettings;
    [SerializeField] private int maxPoolCountPerPrefab = 500;

    private Dictionary<Terrain, float[,,]> terrainAlphaMapCache = new();
    private Dictionary<Terrain, int> terrainTextureIndexCache = new();

    protected override IEnumerator InitAndSpawnLoop()
    {
        yield return CacheTerrainAlphamaps();
        yield return base.LoadPrefabs();

        foreach (var prefab in loadedPrefabs)
        {
            Runner?.SetMaxPool(prefab.name, maxPoolCountPerPrefab);
        }

        if (loadedPrefabs.Count > 0)
            StartCoroutine(SpawnLoop());
    }

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

    protected override IEnumerator SpawnLoop()
    {
        while (true)
        {
            foreach (var setting in terrainSettings)
            {
                int aliveCount = 0;
                foreach (var obj in setting.activeObjects)
                {
                    if (obj != null && obj.GetComponent<EnvironmentObject>().IsAlive())
                        aliveCount++;
                }

                int needed = setting.maxObjects - aliveCount;
                for (int i = 0; i < needed; i++)
                {
                    TrySpawnOne(setting);
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    private void TrySpawnOne(TerrainSpawnSettings setting)
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

            SpawnObject(worldPos, Quaternion.Euler(0, Random.Range(0, 360), 0), setting.activeObjects);
            return;
        }
    }

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
        return alphamaps[mapZ, mapX, layerIndex] > 0.5f;
    }

    private bool IsOverlapping(Vector3 pos, TerrainSpawnSettings setting)
    {
        foreach (var obj in setting.activeObjects)
        {
            if (obj != null && obj.GetComponent<EnvironmentObject>().IsAlive() &&
                Vector3.Distance(obj.transform.position, pos) < setting.minDistanceBetweenObjects)
                return true;
        }
        return false;
    }
}
