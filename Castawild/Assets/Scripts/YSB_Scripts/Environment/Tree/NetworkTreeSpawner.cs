using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using YSB_Scripts;
using Test;

public class NetworkTreeSpawner : EnvironmentSpawner<YSB_Scripts.NetworkTree, TreeDefinition>
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

        // 풀 최대 개수 세팅 (base 클래스에서 loadedPrefabs 채워진 뒤에 호출 가능)
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
