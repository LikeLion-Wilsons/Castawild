using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Test
{
    public class NetworkTreeSpawner : NetworkBehaviour
    {
        [SerializeField] private NetworkTree _treePrefab;

        private List<NetworkTree> _tree = new();

        public override void Spawned()
        {
            if (HasStateAuthority == false) return;


            var tree = Runner.Spawn(_treePrefab, new Vector3(2, 0, 2), Quaternion.identity, Object.InputAuthority, (runner, o) =>
            {
                o.GetComponent<NetworkTree>().health.Init(100);
            });
            tree.transform.SetParent(transform);
            _tree.Add(tree);
        }
    }
}