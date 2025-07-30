using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class CHHNetworkTreeSpawner : NetworkBehaviour
{
    [SerializeField] private CHHNetworkTree _treePrefab;
    [SerializeField] private Transform[] points;
    private List<CHHNetworkTree> trees = new List<CHHNetworkTree>();

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
            o.GetComponent<CHHNetworkTree>().Init(30);
        });
        tree.transform.SetParent(transform);
        trees.Add(tree);
    }
}
