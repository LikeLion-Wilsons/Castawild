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

        private NetworkTree instanceTree;

        public override void Spawned()
        {
            if (HasStateAuthority == false) return;
            SpawnTree();
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false) return;
            if (instanceTree == null)
            {
                SpawnTree();
            }
        }

        void SpawnTree()
        {
            var x= UnityEngine.Random.Range(0, 3f);
            var z= UnityEngine.Random.Range(-3f, 3f);
            instanceTree = Runner.Spawn(_treePrefab, new Vector3(x, 1, z), Quaternion.identity, null, (runner, o) =>
            {
                o.GetComponent<NetworkTree>().Init(100);
            });
            instanceTree.transform.SetParent(transform);
        }
    }
}