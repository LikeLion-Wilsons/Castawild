using System.Collections;
using UnityEngine;
using Test;
public class NetworkStoneSpawner : EnvironmentSpawner<YSB_Scripts.NetworkStone, StoneDefinition>
{
    [SerializeField, Tooltip("트리 프리팹 최대 풀 개수. 0이면 무한, -1이면 풀링 안 함")]
    private int maxPoolCountPerPrefab = 500;

    public override void Spawned()
    {
        if (HasStateAuthority)
            StartCoroutine(InitAndSpawnLoop());
    }

    protected override IEnumerator InitAndSpawnLoop()
    {
        yield return base.CacheTerrainAlphamaps();
        yield return base.LoadPrefabs();

        foreach (var prefab in loadedPrefabs)
        {
            if (Runner != null && prefab != null)
            {
                Runner.SetMaxPool(prefab.name, maxPoolCountPerPrefab);
            }
        }

        if (loadedPrefabs.Count > 0)
            StartCoroutine(base.SpawnLoop());
    }
}
