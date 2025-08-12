using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(NetworkObject))]
public abstract class EnvironmentSpawner<T, U> : NetworkBehaviour
    where T : EnvironmentObject
    where U : SpawnableDefinition
{
    [Header("Definitions")]
    [SerializeField] protected List<U> definitions;

    [Header("Spawn Settings")]
    [SerializeField] protected int maxSpawnAttempts = 5;
    [SerializeField] protected float checkInterval = 3f;

    protected Dictionary<GameObject, U> prefabToDefinitionMap = new();
    protected List<GameObject> loadedPrefabs = new();
    protected int nextInstanceId = 1;

    public override void Spawned()
    {
        if (HasStateAuthority)
            StartCoroutine(InitAndSpawnLoop());
    }

    protected virtual IEnumerator InitAndSpawnLoop()
    {
        yield return LoadPrefabs();

        if (loadedPrefabs.Count > 0)
            StartCoroutine(SpawnLoop());
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

    protected abstract IEnumerator SpawnLoop();

    protected void SpawnObject(Vector3 pos, Quaternion rotation, IList<NetworkObject> activeList)
    {
        var prefab = loadedPrefabs[Random.Range(0, loadedPrefabs.Count)];
        if (!prefabToDefinitionMap.TryGetValue(prefab, out var def)) return;

        NetworkObject netObj = Runner.Spawn(
            prefab,
            pos,
            rotation,
            onBeforeSpawned: (runner, obj) =>
            {
                var spawnable = obj.GetComponent<T>();
                spawnable.Init(def, nextInstanceId++);
            }
        );

        if (netObj != null)
        {
            Runner.MoveToRunnerScene(netObj.gameObject);
            if (netObj.HasStateAuthority)
            {
                activeList.Add(netObj);
            }
        }
    }
}
