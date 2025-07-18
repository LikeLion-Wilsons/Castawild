using Fusion;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class NetworkTreeSpawnerTest : NetworkBehaviour
{
    [SerializeField] private NetworkTreeTest _treePrefab;
    [SerializeField] private Transform[] points;
    private List<NetworkTreeTest> trees = new List<NetworkTreeTest>();

    public override void Spawned()
    {
        for (int i = 0; i < points.Length; i++)
        {
            SpawnTree(points[i].position);
        }

    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority == false) return;
        for (int i = 0; i < trees.Count; i++)
        {
            if (trees[i].IsAlive == false && trees[i].CanRevive())
            {
                trees[i].Revive();
            }
        }
    }

    void SpawnTree(Vector3 spawnPos)
    {
        if (HasStateAuthority == false) return;
        var tree = Runner.Spawn(_treePrefab, spawnPos, Quaternion.identity, null, (runner, o) =>
        {
            o.GetComponent<NetworkTreeTest>().Init(30);
        });
        tree.transform.SetParent(transform);
        trees.Add(tree);
    }

}
