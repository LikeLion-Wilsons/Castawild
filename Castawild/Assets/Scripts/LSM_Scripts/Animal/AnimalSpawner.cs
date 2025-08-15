using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : NetworkBehaviour
{
    [SerializeField] private CwAnimal _AnimalPrefab;
    [SerializeField] private Transform[] points;
    private List<CwAnimal> animals = new List<CwAnimal>(); 

    public override void Spawned()
    {
        for (int i = 0; i < points.Length; i++)
        {
            SpawnAnimal(points[i].position);
        } 
    }

    public override void FixedUpdateNetwork()
    {         

    }

    void ReSpawnAnimal(Vector3 spawnPos)
    {

    }



    void SpawnAnimal(Vector3 spawnPos)
    {
        if (HasStateAuthority == false) return;
        var animal = Runner.Spawn(_AnimalPrefab, spawnPos, Quaternion.identity, null, (runner, o) =>
        {
            o.GetComponent<CwAnimal>().Init();
        }); 
        animals.Add(animal);
    }
}

